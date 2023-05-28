using System.Diagnostics;
using System.Text.RegularExpressions;
using Broker.API.Configuracoes;
using Broker.API.Dados;
using Broker.API.Dados.Dtos;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Broker.API.Ranqueamento;

[ApiController]
[Route("webhook/kapacitor")]
public class WebhookKapacitorController : ControllerBase
{
    private readonly ILogger<WebhookKapacitorController> _logger;
    private readonly MetricasRepository _metricasRepository;
    private readonly BrokerSettings _settings;
    private readonly IDatabase _redisDb;

    public WebhookKapacitorController(
        ILogger<WebhookKapacitorController> logger,
        IConnectionMultiplexer redis,
        MetricasRepository metricasRepository,
        IConfiguration configuration)
    {
        _logger = logger;
        _metricasRepository = metricasRepository;
        _settings = ConfiguracoesUtils.ObterConfiguracoesBroker(configuration);
        _redisDb = redis.GetDatabase();
    }

    [HttpPost]
    public async Task<IActionResult> AtualizarStatusProvedores([FromBody] KapacitorAlertDto alerta)
    {
        var watch = Stopwatch.StartNew();
        
        var evento = alerta.id;
        var level = alerta.level;
        var nomeRecurso = alerta.data.series.First().tags.nome_recurso;
        var nomeProvedor = alerta.data.series.First().tags.nome_provedor;
        _logger.LogInformation(
            "Recebido. Evento={Evento}. Level={Level}. NomeRecurso={NomeRecurso}. NomeProvedor={NomeProvedor}",
            evento, level, nomeRecurso, nomeProvedor);

        if (evento.Contains("status_provedor_") && level == "INFO")
            await TratarProvedorDisponivel(nomeRecurso, nomeProvedor);
        else if (evento.Contains("status_provedor_") && level == "CRITICAL")
            await TratarProvedorIndisponivel(nomeRecurso, nomeProvedor);
        else if (evento.Contains("performance_degradada"))
        {
            var indexLatencia = alerta.data.series.First().columns.IndexOf("latencia");
            var valorLatencia = alerta.data.series.First().values[indexLatencia].ToString();
            ArgumentException.ThrowIfNullOrEmpty(valorLatencia);

            var latenciaMedia = double.Parse(Regex.Replace(valorLatencia, @"[^\d]", ""));
            await TratarProvedorComPerformanceRuim(nomeRecurso, nomeProvedor, latenciaMedia);
        }

        watch.Stop();
        if (_settings.GravarLogsPerformance)
            LogPerformanceCodigo(watch.ElapsedMilliseconds);
        
        return Ok();
    }

    private async Task TratarProvedorDisponivel(string nomeRecurso, string nomeProvedor)
    {
        await _redisDb.SortedSetAddAsync(nomeRecurso, new SortedSetEntry[] { new(nomeProvedor, 0) });
    }

    private async Task TratarProvedorIndisponivel(string nomeRecurso, string nomeProvedor)
    {
        await _redisDb.SortedSetRemoveAsync(nomeRecurso, nomeProvedor);
    }

    private async Task TratarProvedorComPerformanceRuim(string nomeRecurso, string nomeProvedor, double latenciaMedia)
    {
        await _redisDb.SortedSetDecrementAsync(nomeRecurso, nomeProvedor, latenciaMedia);
    }

    private void LogPerformanceCodigo(long tempoProcessamento)
    {
        var logDto = new LogPerformanceCodigoDto
        {
            NomeComponente = "Webhook-Kapacitor",
            TempoProcessamento = tempoProcessamento
        };
        _metricasRepository.LogPerformanceCodigo(logDto);
    }
}