using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Broker.API.Broker;
using Broker.API.Configuracoes;
using Broker.API.Dados;
using Broker.API.Dados.Dtos;
using Newtonsoft.Json;
using Serilog;

namespace Broker.API.Mapeamento;

public class Mapeador
{
    private readonly MetricasRepository _metricasRepository;

    public Mapeador(MetricasRepository metricasRepository)
    {
        _metricasRepository = metricasRepository;
    }

    #region Requisicao

    public HttpRequestMessage MapearRequisicao(
        HttpContext contextoOriginal,
        SolicitacaoDto solicitacao,
        ProvedorSettings provedorAlvo,
        bool logPerformance = false)
    {
        var watch = Stopwatch.StartNew();

        var uri = CriarUriAlvo(provedorAlvo.Rota, solicitacao.ParametrosRota);
        var requisicaoMapeada = CriarRequisicao(contextoOriginal, uri, provedorAlvo);

        watch.Stop();
        if (logPerformance)
            LogPerformanceCodigo("Requisicao", watch.ElapsedMilliseconds);

        return requisicaoMapeada;
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

    private HttpRequestMessage CriarRequisicao(HttpContext contextoOriginal,
        Uri uriAlvo, ProvedorSettings provedorSettings)
    {
        var requisicao = new HttpRequestMessage();
        CopiarHeadersRequisicaoOriginal(contextoOriginal, requisicao);

        requisicao.RequestUri = uriAlvo;
        requisicao.Headers.Host = uriAlvo.Host;
        requisicao.Method = ObterMetodoHttp(provedorSettings.Metodo ?? contextoOriginal.Request.Method);

        return requisicao;
    }

    private void CopiarHeadersRequisicaoOriginal(
        HttpContext context,
        HttpRequestMessage requestMessage)
    {
        var metodo = context.Request.Method;

        if (!HttpMethods.IsGet(metodo) &&
            !HttpMethods.IsHead(metodo) &&
            !HttpMethods.IsDelete(metodo) &&
            !HttpMethods.IsTrace(metodo))
        {
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

    public RespostaMapeada MapearResposta(
        HttpResponseMessage respostaProvedor,
        ProvedorSettings provedorAcionado,
        IEnumerable<string> camposEsperados,
        bool logPerformance = false)
    {
        var watch = Stopwatch.StartNew();

        var conteudoMapeado = SubstituirCamposParaCliente(respostaProvedor,
            provedorAcionado, camposEsperados);

        if (conteudoMapeado != null)
            conteudoMapeado["provedor"] = provedorAcionado.Nome;

        var respostaMapeada = new RespostaMapeada
        {
            HttpResponseMessage = GerarHttpResponseMessage(conteudoMapeado, respostaProvedor),
            CamposMapeados = conteudoMapeado
        };

        watch.Stop();
        if (logPerformance)
            LogPerformanceCodigo("Resposta", watch.ElapsedMilliseconds);

        return respostaMapeada;
    }

    private Dictionary<string, string> SubstituirCamposParaCliente(
        HttpResponseMessage respostaProvedor,
        ProvedorSettings provedor,
        IEnumerable<string> camposEsperados)
    {
        var respostaString = respostaProvedor.Content.ReadAsStringAsync().Result;

        var json = ParseJsonResposta(respostaString);
        if (json is null)
        {
            Log.Warning("Erro ao mapear resposta do provedor");
            return new Dictionary<string, string>();
        }

        var campos = new Dictionary<string, string>();

        foreach (var campoEsperado in camposEsperados)
        {
            var formato = provedor.FormatoResposta
                .FirstOrDefault(x => x.NomeDesejado == campoEsperado);

            var path = formato?.NomeRecebido.Split('.') ?? new[] { campoEsperado };

            var prop = json;
            foreach (var s in path)
            {
                var valid = prop.Value.TryGetProperty(s, out var propOut);
                if (valid)
                    prop = propOut;
                else
                {
                    Log.Warning("Falha no mapeamento: campo {NomeCampo} não retornado pelo provedor", s);
                    return null;
                }
            }

            campos.Add(formato?.NomeDesejado ?? campoEsperado, prop.Value.GetString());
        }

        return campos;
    }

    private JsonElement? ParseJsonResposta(string resposta)
    {
        try
        {
            return JsonDocument.Parse(resposta).RootElement;
        }
        catch (Exception)
        {
            Log.Warning("O formato da resposta não pode ser convertido para JSON");
            return null;
        }
    }

    private HttpResponseMessage GerarHttpResponseMessage(Dictionary<string, string> campos,
        HttpResponseMessage respostaProvedor)
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

    private void LogPerformanceCodigo(string etapa, long tempoProcessamento)
    {
        var logDto = new LogPerformanceCodigoDto
        {
            NomeComponente = $"Mapeador-{etapa}",
            TempoProcessamento = tempoProcessamento
        };
        _metricasRepository.LogPerformanceCodigo(logDto);
    }
}