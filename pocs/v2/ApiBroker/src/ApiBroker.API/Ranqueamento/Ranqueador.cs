using ApiBroker.API.Broker;
using ApiBroker.API.Dados;

namespace ApiBroker.API.Ranqueamento;

public class Ranqueador
{
    public async Task<List<string>> ObterOrdemMelhoresProvedores(SolicitacaoDto solicitacao)
    {
        var nomeRecurso = solicitacao.Nome;

        var provedores = await ObterTodosProvedores(nomeRecurso);

        var criterios = solicitacao.Criterios;
        var provedoresDisponiveis = provedores
            .Where(x => (int)x["error_rate"] < criterios.ErrorBudgetHora)
            .OrderBy(x => x["response_time"])
            .Select(p => (string)p["name"])
            .ToList();

        return !provedoresDisponiveis.Any() ? new List<string>() : provedoresDisponiveis;
    }

    private async Task<List<Dictionary<string, object>>> ObterTodosProvedores(string nomeRecurso)
    {
        var metricasDao = new MetricasDao();
        return await metricasDao.ObterDadosProvedores(nomeRecurso);
    }

}