using ApiBroker.API.Configuracoes;

namespace ApiBroker.API.Broker;

public class Identificador
{
    private readonly ILogger<Identificador> _logger;

    public Identificador()
    {
        _logger = LoggerFactory.Factory().CreateLogger<Identificador>();
    }
    
    /// <summary>
    /// Identifica o recurso solicitado pelo cliente com base no nome enviado na requisição
    /// </summary>
    /// <param name="rota">Rota da requisição que o cliente fez no Broker</param>
    /// <param name="configuracoes">Configurações definidas pelo cliente</param>
    /// <returns>Recurso solicitado pelo cliente</returns>
    public SolicitacaoDto IdentificarRecursoSolicitado(PathString rota, IConfiguration configuracoes)
    {
        _logger.LogInformation("Iniciando identificação do recurso solicitado");
        
        if (!rota.StartsWithSegments("/api", out var infoRequisicao))
            return null;

        if (infoRequisicao.Value == null)
        {
            _logger.LogWarning(
                "A requisição não está no formato correto. " +
                "Após '/api', é obrigatório informar o nome do recurso desejado");
            return null;
        }
        
        var infosRequisicao = infoRequisicao.Value.Split("/")[1..];
        
        var nomeRecurso = infosRequisicao[0];
        var recurso = ConfiguracoesUtils.ObterRecursoPeloNome(nomeRecurso, configuracoes);
        if (recurso is null)
        {
            _logger.LogWarning("Não há configurações para o recurso solicitado: {NomeRecurso}", nomeRecurso);
            return null;
        }

        var parametrosRotaEsperados = recurso.ParametrosViaRota;
        var parametrosRotaRecebidos = infosRequisicao[1..];
        var parametrosRotaMapeados = ObterParametrosRota(parametrosRotaEsperados, parametrosRotaRecebidos);
        if (parametrosRotaMapeados is null)
        {
            _logger.LogWarning("Há parâmetros de rota configurados para esse recurso, mas não foram enviados corretamente na requisição");
            return null;
        }
        
        _logger.LogInformation("Recurso identificado com o nome {NomeRecurso}", recurso.Nome);

        return new SolicitacaoDto
        {
            NomeRecurso = recurso.Nome,
            Provedores = recurso.Provedores,
            ParametrosRota = parametrosRotaMapeados,
            CamposResposta = recurso.CamposResposta,
            Criterios = recurso.Criterios,
            TentarTodosProvedoresAteSucesso = recurso.TentarTodosProvedoresAteSucesso
        };
    }

    private Dictionary<string, string> ObterParametrosRota(IReadOnlyList<string> nomesParametrosRota, IReadOnlyList<string> valoresParametrosRota)
    {
        if (nomesParametrosRota.Count != valoresParametrosRota.Count)
            return null;

        var parametros = new Dictionary<string, string>();
        for (var i = 0; i < nomesParametrosRota.Count; i++)
        {
            var nome = nomesParametrosRota[i];
            var valor = valoresParametrosRota[i];
            parametros[nome] = valor;
        }

        return parametros;
    }
}