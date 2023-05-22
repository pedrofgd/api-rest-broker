using System.Diagnostics;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using InfluxDb.Kapacitor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();

var app = builder.Build();

using var influx = new InfluxDBClient("http://localhost:8086", builder.Configuration["InfluxDbToken"]);
var writeApi = influx.GetWriteApiAsync();

app.MapGet("/{nomeRecurso}/{nomeProvedor}/{success}", async (string nomeRecurso,
    string nomeProvedor, bool success) =>
{
    var watch = Stopwatch.StartNew();
    
    Console.WriteLine($"GET /{success}");

    await Task.Delay(250);

    var point = PointData.Measurement("performance_influx")
        .Tag("nome_recurso", nomeRecurso)
        .Tag("nome_provedor", nomeProvedor)
        .Field("latencia", watch.ElapsedMilliseconds)
        .Field("sucesso", success ? 1 : 0)
        .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

    await writeApi.WritePointAsync(point, "logs", "broker");
});

app.MapPost("/webhook", ([FromBody] KapacitorAlertDto alert, IMemoryCache memoryCache) =>
{
    var evento = alert.id;
    var level = alert.level;
    var nomeRecurso = alert.data.series.First().tags.nome_recurso;
    var nomeProvedor = alert.data.series.First().tags.nome_provedor;
    Console.WriteLine($"Recebido. Evento={evento}. Level={level}. NomeRecurso={nomeRecurso}. NomeProvedor={nomeProvedor}");

    if (evento.Contains("status_provedor"))
    {
        var recursoEmCache = memoryCache.TryGetValue(nomeRecurso, out string? provedores);
        
        if (level == "CRITICAL")
        {
            if (recursoEmCache && provedores != null)
            {
                var provedoresDisponiveis = provedores.Split(",").ToList();
                var indexProvedorIndisponivel = provedoresDisponiveis.SingleOrDefault(p => p == nomeProvedor);
                if (indexProvedorIndisponivel != null)
                    provedoresDisponiveis.Remove(indexProvedorIndisponivel);
                memoryCache.Set(nomeRecurso, string.Join(",", provedoresDisponiveis));
            }
        }

        else if (level == "INFO")
        {
            var provedoresDisponiveis = new List<string>();

            if (provedores != null)
            {
                provedoresDisponiveis = provedores.Split(",").ToList();
                provedoresDisponiveis.Add(nomeProvedor);
            }
            else
                provedoresDisponiveis.Add(nomeProvedor);

            var listaAtualizada = string.Join(",", provedoresDisponiveis);
            memoryCache.Set(nomeRecurso, listaAtualizada);
        }
    }
});

app.MapGet("/provedores/{nomeRecurso}", (string nomeRecurso, IMemoryCache memoryCache) =>
{
    var recursoEmCache = memoryCache.TryGetValue(nomeRecurso, out string? provedoresDisponiveis);
    if (recursoEmCache)
    {
        var mensagemProvedoresDisponiveis = string.IsNullOrEmpty(provedoresDisponiveis)
            ? "nenhum"
            : provedoresDisponiveis;
        Console.WriteLine($"Provedores disponiveis = {mensagemProvedoresDisponiveis}");
    }
    else
    {
        Console.WriteLine($"Não há conteúdo em cache para o recurso {nomeRecurso}");
    }
});

app.Run();