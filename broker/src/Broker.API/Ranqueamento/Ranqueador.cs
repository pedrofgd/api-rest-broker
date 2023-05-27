using System.Diagnostics;
using Broker.API.Broker;
using Broker.API.Dados;
using Broker.API.Dados.Dtos;
using Serilog;
using StackExchange.Redis;

namespace Broker.API.Ranqueamento;

public class Ranqueador
{
    private readonly MetricasRepository _metricasRepository;
    private readonly IDatabase _redisDb;

    public Ranqueador(
        MetricasRepository metricasRepository,
        IConnectionMultiplexer redis)
    {
        _metricasRepository = metricasRepository;
        _redisDb = redis.GetDatabase();
    }

    public async Task<List<string>> ObterOrdemMelhoresProvedores(
        SolicitacaoDto solicitacao,
        bool logPerformance = false)
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
        if (logPerformance)
        {
            LogPerformanceCodigo(watch.ElapsedMilliseconds);
            LogAcompanhamentoProvedores(ordemProvedores.Count,
                string.Join(",", ordemProvedores));
        }

        return !provedoresDisponiveis.Any() ? new List<string>() : ordemProvedores;
    }

    private void LogPerformanceCodigo(long tempoProcessamento)
    {
        var logDto = new LogPerformanceCodigoDto
        {
            NomeComponente = "Ranqueador",
            TempoProcessamento = tempoProcessamento
        };
        _metricasRepository.LogPerformanceCodigo(logDto);
    }

    private void LogAcompanhamentoProvedores(int qtdeProvedoresDisponiveis, string provedoresDisponiveis)
    {
        _metricasRepository.LogAcompanhamentoProvedores(qtdeProvedoresDisponiveis, provedoresDisponiveis);
    }
}