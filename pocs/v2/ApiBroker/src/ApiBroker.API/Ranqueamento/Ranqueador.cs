using ApiBroker.API.Broker;
using ApiBroker.API.Dados;

namespace ApiBroker.API.Ranqueamento;

public class Ranqueador
{
    private readonly ILogger<Ranqueador> _logger;

    public Ranqueador()
    {
        _logger = LoggerFactory.Factory().CreateLogger<Ranqueador>();
    }
    
    public async Task<List<string>> ObterOrdemMelhoresProvedores(SolicitacaoDto solicitacao, IConfiguration configuration)
    {
        _logger.LogInformation("Iniciando processo para obter ordem dos provedores");
        
        var nomeRecurso = solicitacao.NomeRecurso;

        var provedores = await ObterTodosProvedores(nomeRecurso, configuration);

        var criterios = solicitacao.Criterios;
        var provedoresDisponiveis = provedores
            .Where(x => (int)x["error_rate"] < criterios.ErrorBudgetHora)
            .OrderBy(x => x["response_time"])
            .Select(p => (string)p["name"])
            .ToList();

        _logger.LogInformation(
            "Ordem dos melhores provedores obtida. " +
            "Há {QtdeProvedoresDisponiveis} provedores que atendem os critérios",
            provedoresDisponiveis.Count);

        return !provedoresDisponiveis.Any() ? new List<string>() : provedoresDisponiveis;
    }

    private async Task<List<Dictionary<string, object>>> ObterTodosProvedores(string nomeRecurso, IConfiguration configuration)
    {
        var metricasDao = new MetricasDao();
        return await metricasDao.ObterDadosProvedores(nomeRecurso, configuration);
    }

}