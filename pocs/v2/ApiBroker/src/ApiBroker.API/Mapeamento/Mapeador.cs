using System.Text;
using System.Text.Json;
using ApiBroker.API.Broker;
using ApiBroker.API.Configuracoes;
using Newtonsoft.Json;

namespace ApiBroker.API.Mapeamento;

public class Mapeador
{
    #region Requisicao

    /// <summary>
    /// Faz o mapeamento da requisição feita pelo cliente para o formato esperado pelo provedor alvo 
    /// </summary>
    /// <param name="contextoOriginal">Requisição original feita pelo cliente</param>
    /// <param name="solicitacao">Dados da solicitação feita pelo cliente ao Broker</param>
    /// <param name="provedorAlvo">Provedor que receberá a requisição</param>
    /// <returns></returns>
    public HttpRequestMessage MapearRequisicao(HttpContext contextoOriginal, SolicitacaoDto solicitacao, ProvedorSettings provedorAlvo)
    {
        var uri = CriarUriAlvo(provedorAlvo.Rota, solicitacao.ParametrosRota);
        return CriarRequisicao(contextoOriginal, uri);
    }

    private Uri CriarUriAlvo(string rota, Dictionary<string, string> parametrosRota)
    {
        rota = SubstituirParametrosRota(rota, parametrosRota);
        return new Uri(rota);
    }

    private string SubstituirParametrosRota(string rota, Dictionary<string, string> parametrosRota)
    {
        foreach (var parametro in parametrosRota)
            rota = rota.Replace("{" + parametro.Key + "}", parametro.Value);

        return rota;
    }
    
    private HttpRequestMessage CriarRequisicao(HttpContext contextoOriginal, Uri uriAlvo)
    {
        var requisicao = new HttpRequestMessage();
        CopiarHeadersRequisicaoOriginal(contextoOriginal, requisicao);

        requisicao.RequestUri = uriAlvo;
        requisicao.Headers.Host = uriAlvo.Host;
        requisicao.Method = ObterMetodoHttp(contextoOriginal.Request.Method);

        return requisicao;
    }

    private void CopiarHeadersRequisicaoOriginal(HttpContext context, HttpRequestMessage requestMessage)
    {
        // todo: ajustar para utilizar o método configurado pelo cliente, caso exista
        var metodo = context.Request.Method;
        
        if (!HttpMethods.IsGet(metodo) &&
            !HttpMethods.IsHead(metodo) &&
            !HttpMethods.IsDelete(metodo) &&
            !HttpMethods.IsTrace(metodo))
        {
            /*
             * todo: implementar mapeamento do body da requisição e query params
             *  Por enquanto os testes estão sendo feitos apenas enviando parâmetros via rota
             */
            var streamContent = new StreamContent(context.Request.Body);
            requestMessage.Content = streamContent;
        }

        foreach (var header in context.Request.Headers)
        {
            requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }
    
    private static HttpMethod ObterMetodoHttp(string method)
    {
        if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
        if (HttpMethods.IsGet(method)) return HttpMethod.Get;
        if (HttpMethods.IsHead(method)) return HttpMethod.Head;
        if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
        if (HttpMethods.IsPost(method)) return HttpMethod.Post;
        if (HttpMethods.IsPut(method)) return HttpMethod.Put;
        if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
        return new HttpMethod(method);
    }
    
    #endregion

    #region Resposta

    /// <summary>
    /// Faz o mapeamento do resposta do provedor para a resposta esperada pelo cliente
    /// </summary>
    /// <param name="respostaProvedor">Conteúdo retornado pelo provedor na requisição</param>
    /// <param name="provedorAcionado">Provedor que retornou a resposta</param>
    /// <param name="camposEsperados">Lista de campos que o cliente espera receber</param>
    /// <returns>Resposta mapeada com base nas configurações do Cliente</returns>
    public RespostaMapeada MapearResposta(HttpResponseMessage respostaProvedor, ProvedorSettings provedorAcionado, string[] camposEsperados)
    {
        var conteudoMapeado = SubstituirCamposParaCliente(respostaProvedor, provedorAcionado, camposEsperados);

        // todo: avaliar incluir parse dos campos também, para tipos especificados pelo cliente

        conteudoMapeado["provedor"] =  provedorAcionado.Nome;

        return new RespostaMapeada
        {
            HttpResponseMessage = GerarHttpResponseMessage(conteudoMapeado, respostaProvedor),
            CamposMapeados = conteudoMapeado
        };
    }

    private Dictionary<string, string> SubstituirCamposParaCliente(HttpResponseMessage respostaProvedor, ProvedorSettings provedor, string[] camposEsperados)
    {
        var respostaString = respostaProvedor.Content.ReadAsStringAsync().Result;
    
        var json = JsonDocument.Parse(respostaString).RootElement;

        var campos = new Dictionary<string, string>();

        foreach (var campoEsperado in camposEsperados)
        {
            var formato = provedor.FormatoResposta
                .FirstOrDefault(x => x.NomeDesejado == campoEsperado);

            var path = formato?.NomeRecebido.Split('.') ?? new[] { campoEsperado };
            
            var prop = json;
            foreach (var s in path)
                prop = prop.GetProperty(s);

            campos.Add(formato?.NomeDesejado ?? campoEsperado, prop.GetString());
        }

        return campos;
    }

    private HttpResponseMessage GerarHttpResponseMessage(Dictionary<string, string> campos, HttpResponseMessage respostaProvedor)
    {
        var camposString = JsonConvert.SerializeObject(campos);
        var conteudo = new StringContent(camposString, Encoding.UTF8, "application/json");
        
        var resposta = new HttpResponseMessage();
        foreach (var header in respostaProvedor.Headers)
        {
            resposta.Headers.Add(header.Key, header.Value.ToArray());
        }
        resposta.StatusCode = respostaProvedor.StatusCode;
        resposta.ReasonPhrase = respostaProvedor.ReasonPhrase;
        resposta.Content = conteudo;

        return resposta;
    }

    /// <summary>
    /// Copia os headers da resposta do provedor para retorná-los para o cliente
    /// </summary>
    public void CopiarHeadersRespostaProvedor(HttpContext requisicaoOriginal, HttpResponseMessage respostaProvedor)
    {
        foreach (var header in respostaProvedor.Headers)
        {
            requisicaoOriginal.Response.Headers[header.Key] = header.Value.ToArray();
        }
        
        foreach (var header in respostaProvedor.Content.Headers)
        {
            requisicaoOriginal.Response.Headers[header.Key] = header.Value.ToArray();
        }
        requisicaoOriginal.Response.Headers.Remove("transfer-encoding");
    }
    
    #endregion
}