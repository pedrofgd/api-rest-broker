using System.Diagnostics;
using ApiBroker.API.Broker;
using ApiBroker.API.Dados;
using Serilog;

namespace ApiBroker.API.Ranqueamento;

public class Ranqueador
{
    private readonly MetricasDao _metricasDao;

    public Ranqueador(MetricasDao metricasDao)
    {
        _metricasDao = metricasDao;
    }
    
    public async Task<List<string>> ObterOrdemMelhoresProvedores(SolicitacaoDto solicitacao, IConfiguration configuration)
    {
        var watch = Stopwatch.StartNew();
        
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

        watch.Stop();
        LogPerformanceCodigo(watch.ElapsedMilliseconds);
        
        return !provedoresDisponiveis.Any() ? new List<string>() : provedoresDisponiveis;
    }

    private async Task<List<Dictionary<string, object>>> ObterTodosProvedores(string nomeRecurso, IConfiguration configuration)
    {
        return await _metricasDao.ObterDadosProvedores(nomeRecurso);
    }

    private void LogPerformanceCodigo(long tempoProcessamento)
    {
        var logDto = new LogPerformanceCodigoDto
        {
            NomeComponente = "Ranqueador",
            TempoProcessamento = tempoProcessamento
        };
        _metricasDao.LogPerformanceCodigo(logDto);
    }
}