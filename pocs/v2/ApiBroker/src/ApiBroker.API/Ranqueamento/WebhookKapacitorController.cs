using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace ApiBroker.API.Ranqueamento;

[ApiController]
[Route("webhook/kapacitor")]
public class WebhookKapacitorController : ControllerBase
{
    private readonly ILogger<WebhookKapacitorController> _logger;
    private readonly IDatabase _redisDb;

    public WebhookKapacitorController(
        ILogger<WebhookKapacitorController> logger,
        IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redisDb = redis.GetDatabase();
    }
    
    [HttpPost]
    public void AtualizarStatusProvedores([FromBody] KapacitorAlertDto alerta)
    {
        var evento = alerta.id;
        var level = alerta.level;
        var nomeRecurso = alerta.data.series.First().tags.nome_recurso;
        var nomeProvedor = alerta.data.series.First().tags.nome_provedor;
        _logger.LogInformation($"Recebido. Evento={evento}. Level={level}. NomeRecurso={nomeRecurso}. NomeProvedor={nomeProvedor}");
        
        if (evento.Contains("status_provedor_") && level == "INFO")
            TratarProvedorDisponivel(nomeRecurso, nomeProvedor);
        else if (evento.Contains("status_provedor_") && level == "CRITICAL")
            TratarProvedorIndisponivel(nomeRecurso, nomeProvedor);
        else if (evento.Contains("performance_degradada"))
        {
            var indexLatencia = alerta.data.series.First().columns.IndexOf("latencia");
            var valorLatencia = alerta.data.series.First().values[indexLatencia].ToString();
            ArgumentException.ThrowIfNullOrEmpty(valorLatencia);
            
            var latenciaMedia = double.Parse(Regex.Replace(valorLatencia, @"[^\d]", ""));
            TratarProvedorComPerformanceRuim(nomeRecurso, nomeProvedor, latenciaMedia);
        }
    }

    private void TratarProvedorDisponivel(string nomeRecurso, string nomeProvedor)
    {
        _redisDb.SortedSetAdd(nomeRecurso, new SortedSetEntry[] { new(nomeProvedor, 0) });   
    }

    private void TratarProvedorIndisponivel(string nomeRecurso, string nomeProvedor)
    {
        _redisDb.SortedSetRemove(nomeRecurso, nomeProvedor);
    }

    private void TratarProvedorComPerformanceRuim(string nomeRecurso, string nomeProvedor, double latenciaMedia)
    {
        // todo: melhorar a forma de ordenar, pois o valor do score pode diminuir demais após muito tempo com a aplicação rodando (precisaria resetar)
        _redisDb.SortedSetDecrement(nomeRecurso, nomeProvedor, latenciaMedia);
    }
}