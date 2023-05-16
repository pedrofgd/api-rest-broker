using ApiBroker.API.Broker;
using ApiBroker.API.Dados;
using Serilog;

namespace ApiBroker.API.Ranqueamento;

public class Ranqueador
{
    public async Task<List<string>> ObterOrdemMelhoresProvedores(SolicitacaoDto solicitacao, IConfiguration configuration)
    {
        Log.Information("Iniciando processo para obter ordem dos provedores");
        
        var nomeRecurso = solicitacao.NomeRecurso;

        var provedores = await ObterTodosProvedores(nomeRecurso, configuration);

        var criterios = solicitacao.Criterios;
        var provedoresDisponiveis = provedores
            .Where(x => (int)x["error_rate"] < criterios.ErrorBudgetHora)
            .OrderBy(x => x["response_time"])
            .Select(p => (string)p["name"])
            .ToList();

        Log.Information(
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