using ApiBroker.API.Configuracoes;
using ApiBroker.API.Identificacao;
using ApiBroker.API.Mapeamento;
using ApiBroker.API.Requisicao;
using ApiBroker.API.Monitoramento;
using ApiBroker.API.Validacao;

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
        
        var resultadoValido = false;
        while (!resultadoValido)
        {
            var provedorAlvo = ObterProvedorAlvo(solicitacao.Provedores);
            if (provedorAlvo is null)
                return;
            // todo: validar se todos os provedores já foram chamados

            var requisicao = mapeador.MapearRequisicao(context, solicitacao, provedorAlvo);

            var requisitor = new Requisitor();
            var (respostaProvedor, tempoRespostaMs) = await requisitor.EnviarRequisicao(requisicao);

            respostaMapeada = mapeador.MapearResposta(respostaProvedor, provedorAlvo, solicitacao.CamposResposta, context);

            LogResultado(solicitacao, provedorAlvo, respostaProvedor, tempoRespostaMs);

            var validador = new Validador(solicitacao);
            resultadoValido = validador.Validar(respostaMapeada);
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

    /// <summary>
    /// Obtém o provedor mais disponível para atender a requisição do cliente
    /// </summary>
    /// <param name="provedores">Lista de provedores configurada para o recurso</param>
    /// <returns>Configurações do provedor mais disponível</returns>
    private ProvedorSettings? ObterProvedorAlvo(IReadOnlyList<ProvedorSettings> provedores)
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