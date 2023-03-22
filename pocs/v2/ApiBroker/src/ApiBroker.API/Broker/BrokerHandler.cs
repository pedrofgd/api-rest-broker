using ApiBroker.API.Configuracoes;
using ApiBroker.API.Identificacao;
using ApiBroker.API.Mapeamento;

namespace ApiBroker.API.Broker;

public class BrokerHandler
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public BrokerHandler(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        var recurso = ObterRecursoSolicitado(context.Request.Path);
        if (recurso is null)
            return;
        
        var provedorAlvo = ObterProvedorAlvo(recurso.Provedores);
        if (provedorAlvo is null) 
            return;
        
        // todo: pendente de incluir os parâmetros da requisição (rota e/ou body)

        var respostaProvedor = await EnviarRequisicaoProvedor(provedorAlvo, context);

        var mapeador = new Mapeador();
        var respostaMapeada = mapeador.MapearResponse(respostaProvedor, provedorAlvo, recurso.CamposResposta);
        if (respostaMapeada is null) 
            return;
        
        /*
         * todo: pendente de incluir o mapeamento dos headers e status code + validar o resultado
         *  Avaliar como isso será passado para o mapeador
         *  Validar se a resposta do provedor atingiu os critérios. Caso não chamar o próximo
         */
        await respostaMapeada.Content.CopyToAsync(context.Response.Body);
    }

    /// <summary>
    /// Obtém as configurações do recurso solicitado na rota da requisição
    /// </summary>
    /// <param name="rota">Rota da requisição recebida</param>
    /// <returns>Configurações do recurso solicitado</returns>
    private Recurso? ObterRecursoSolicitado(PathString rota)
    {
        var identificador = new Identificador();
        return identificador.IdentificarRecursoSolicitado(rota, _configuration);
    }

    /// <summary>
    /// Obtém o provedor mais disponível para atender a requisição do cliente
    /// </summary>
    /// <param name="provedores">Lista de provedores configurada para o recurso</param>
    /// <returns>Configurações do provedor mais disponível</returns>
    private Provedor? ObterProvedorAlvo(IReadOnlyList<Provedor> provedores)
    {
        /*
         * todo: pendente de definir e implementar lógica de monitoramento/ranqueamento
         *  Remover o uso do Random() para utilizar o Ranqueador
         */
        
        if (!provedores.Any())
            return null;

        var random = new Random();
        var provedorAleatorio = random.Next(provedores.Count);
        return provedores[provedorAleatorio];
    }

    private async Task<HttpResponseMessage> EnviarRequisicaoProvedor(Provedor provedorAlvo, HttpContext contexto)
    {
        var requisicao = CriarRequisicao(contexto, provedorAlvo.Uri);

        var httpClient = new HttpClient();
        return await httpClient.SendAsync(requisicao);
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
        var metodo = context.Request.Method;
        
        if (!HttpMethods.IsGet(metodo) &&
            !HttpMethods.IsHead(metodo) &&
            !HttpMethods.IsDelete(metodo) &&
            !HttpMethods.IsTrace(metodo))
        {
            /*
             * todo: implementar mapeamento do body da requisição
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
}