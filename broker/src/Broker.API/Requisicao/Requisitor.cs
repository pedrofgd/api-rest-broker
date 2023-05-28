using System.Diagnostics;
using Broker.API.Dados;
using Broker.API.Dados.Dtos;
using Serilog;

namespace Broker.API.Requisicao;

public class Requisitor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MetricasRepository _metricasRepository;

    public Requisitor(
        IHttpClientFactory httpClientFactory,
        MetricasRepository metricasRepository)
    {
        _httpClientFactory = httpClientFactory;
        _metricasRepository = metricasRepository;
    }

    public async Task<Tuple<HttpResponseMessage, long>> EnviarRequisicao(
        HttpRequestMessage requisicao,
        string nomeProvedorAlvo,
        string nomeRecurso,
        bool logPerformance = false)
    {
        var watch = Stopwatch.StartNew();

        try
        {
            using var httpClient = _httpClientFactory.CreateClient("Requisitor");

            Log.Information("Enviando requisição para {Url}", requisicao.RequestUri);
            var resultado = await httpClient.SendAsync(requisicao);

            Log.Information("Requisição realizada no provedor {NomeRecurso}/{NomeProvedor}",
                nomeRecurso, nomeProvedorAlvo);

            if (logPerformance)
                LogPerformanceCodigo(watch.ElapsedMilliseconds);

            return new Tuple<HttpResponseMessage, long>(resultado, watch.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            Log.Error(
                "Erro ao enviar requisição para {NomeRecurso}/{NomeProvedor}. Erro: {MensagemErro}",
                nomeRecurso, nomeProvedorAlvo, e
            );

            LogPerformanceCodigo(watch.ElapsedMilliseconds);

            return new Tuple<HttpResponseMessage, long>(null, watch.ElapsedMilliseconds);
        }
    }

    private void LogPerformanceCodigo(long tempoProcessamento)
    {
        var logDto = new LogPerformanceCodigoDto
        {
            NomeComponente = "Requisitor",
            TempoProcessamento = tempoProcessamento
        };
        _metricasRepository.LogPerformanceCodigo(logDto);
    }
}