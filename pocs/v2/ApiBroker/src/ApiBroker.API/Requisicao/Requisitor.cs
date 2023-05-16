using System.Diagnostics;
using Serilog;

namespace ApiBroker.API.Requisicao;

public class Requisitor
{
    private readonly IHttpClientFactory _httpClientFactory;

    public Requisitor(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<Tuple<HttpResponseMessage, long>> EnviarRequisicao(HttpRequestMessage requisicao, 
        string nomeProvedorAlvo, string nomeRecurso)
    {
        var watch = Stopwatch.StartNew();
        
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("Requisitor");
            
            Log.Information("Enviando requisição para {Url}", requisicao.RequestUri);
            var resultado = await httpClient.SendAsync(requisicao);

            Log.Information("Requisição realizada no provedor {NomeRecurso}/{NomeProvedor}", 
                nomeRecurso, nomeProvedorAlvo);
            return new Tuple<HttpResponseMessage, long>(resultado, watch.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            Log.Error(
                "Erro ao enviar requisição para {NomeRecurso}/{NomeProvedor}. Erro: {MensagemErro}",
                nomeRecurso, nomeProvedorAlvo, e
            );
            return new Tuple<HttpResponseMessage, long>(null, watch.ElapsedMilliseconds);
        }
    }
}
