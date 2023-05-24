using System.Diagnostics;
using ApiBroker.API.Broker;
using ApiBroker.API.Dados;
using Serilog;
using StackExchange.Redis;

namespace ApiBroker.API.Ranqueamento;

public class Ranqueador
{
    private readonly MetricasDao _metricasDao;
    private readonly IDatabase _redisDb;

    public Ranqueador(
        MetricasDao metricasDao,
        IConnectionMultiplexer redis)
    {
        _metricasDao = metricasDao;
        _redisDb = redis.GetDatabase();
    }
    
    public async Task<List<string>> ObterOrdemMelhoresProvedores(SolicitacaoDto solicitacao)
    {
        var watch = Stopwatch.StartNew();
        
        Log.Information("Iniciando processo para obter ordem dos provedores");

        var provedoresDisponiveis =
            await _redisDb.SortedSetRangeByScoreAsync(solicitacao.NomeRecurso, 
                order: Order.Descending);
        
        var ordemProvedores = provedoresDisponiveis
            .Select(provedor => provedor.ToString()).ToList();

        Log.Information(
            "Ordem dos melhores provedores obtida: {OrdemProvedores}",
            string.Join(",", ordemProvedores));
        
        watch.Stop();
        LogPerformanceCodigo(watch.ElapsedMilliseconds);
        LogAcompanhamentoProvedores(ordemProvedores.Count, string.Join(",", ordemProvedores));

        return !provedoresDisponiveis.Any() ? new List<string>() : ordemProvedores;
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
    
    private void LogAcompanhamentoProvedores(int qtdeProvedoresDisponiveis, string provedoresDisponiveis)
    {
        _metricasDao.LogAcompanhamentoProvedores(qtdeProvedoresDisponiveis, provedoresDisponiveis);
    }
}