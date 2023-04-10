using ApiBroker.API.Configuracoes;
using ApiBroker.API.Identificacao;
using ApiBroker.API.Mapeamento;
using ApiBroker.API.Requisicao;
using ApiBroker.API.Monitoramento;
using ApiBroker.API.Ranqueamento;
using ApiBroker.API.Validacao;

namespace ApiBroker.API.Broker;

public class BrokerHandler
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BrokerHandler> _logger;

    public BrokerHandler(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
        _logger = BrokerLoggerFactory.Factory().CreateLogger<BrokerHandler>();
    }

    /// <summary>
    /// Método invocado para manipular a requisição quando
    /// o cliente chama o Broker no endpoint configurado
    /// </summary>
    /// <param name="context">Contexto da requisição</param>
    public async Task Invoke(HttpContext context)
    {
        var solicitacao = ObterRecursoSolicitado(context.Request.Path);
        if (solicitacao is null)
            return;

        var mapeador = new Mapeador();
        RespostaMapeada respostaMapeada = new();

        var listaProvedores = await ObterOrdemMelhoresProvedores(solicitacao.Nome);
        foreach (var provedor in listaProvedores)
        {
            var provedorAlvo = ObterDadosProvedorAlvo(solicitacao.Nome, provedor);
            if (provedorAlvo is null)
                return;

            var requisicao = mapeador.MapearRequisicao(context, solicitacao, provedorAlvo);

            var requisitor = new Requisitor();
            var (respostaProvedor, tempoRespostaMs) = await requisitor.EnviarRequisicao(requisicao);

            respostaMapeada = mapeador.MapearResposta(respostaProvedor, provedorAlvo, solicitacao.CamposResposta, context);

            LogResultado(solicitacao, provedorAlvo, respostaProvedor, tempoRespostaMs);

            var validador = new Validador(solicitacao);
            var resultadoValido = validador.Validar(respostaMapeada);
            if (resultadoValido) break;
            
            // todo: debug
            _logger.LogWarning("Tentando próximo provedor da lista...");
        }
        
        context.Response.StatusCode = (int)respostaMapeada.HttpResponseMessage.StatusCode;
        mapeador.CopiarHeadersRespostaProvedor(context, respostaMapeada.HttpResponseMessage);
        await respostaMapeada.HttpResponseMessage.Content.CopyToAsync(context.Response.Body);
    }

    /// <summary>
    /// Obtém as configurações do recurso solicitado na rota da requisição
    /// </summary>
    /// <param name="rota">Rota da requisição recebida</param>
    /// <returns>Configurações do recurso solicitado</returns>
    private SolicitacaoDto? ObterRecursoSolicitado(PathString rota)
    {
        var identificador = new Identificador();
        return identificador.IdentificarRecursoSolicitado(rota, _configuration);
    }

    private async Task<List<string>> ObterOrdemMelhoresProvedores(string nomeRecurso)
    {
        var ranqueador = new Ranqueador();
        return await ranqueador.ObterOrdemMelhoresProvedores(nomeRecurso);
    }

    /// <summary>
    /// Obtém o provedor mais disponível para atender a requisição do cliente
    /// </summary>
    /// <param name="nomeRecurso">Nome do recurso solicitado pelo cliente</param>
    /// <param name="nomeProvedor">Nome do provedor alvo</param>
    /// <returns>Configurações do provedor mais disponível</returns>
    private ProvedorSettings? ObterDadosProvedorAlvo(string nomeRecurso, string nomeProvedor)
    {
        return ConfiguracoesUtils.ObterDadosProvedorRecurso(nomeRecurso, nomeProvedor, _configuration);
    }

    private void LogResultado(SolicitacaoDto solicitacao, ProvedorSettings provedorAlvo,
        HttpResponseMessage respostaProvedor, long tempoRespostaMs)
    {
        var monitorador = new Monitorador();
        var logDto = new LogDto
        {
            NomeRecurso = solicitacao.Nome,
            NomeProvedor = provedorAlvo.Nome,
            TempoRespostaMs = tempoRespostaMs,
            Sucesso = respostaProvedor.IsSuccessStatusCode,
            Origem = "RequisicaoCliente"
        };
        monitorador.Log(logDto);
    }
}