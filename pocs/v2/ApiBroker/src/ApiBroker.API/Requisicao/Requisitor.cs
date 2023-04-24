using System.Diagnostics;

namespace ApiBroker.API.Requisicao;

public class Requisitor
{
    private readonly ILogger<Requisitor> _logger;
    
    public Requisitor()
    {
        _logger = LoggerFactory.Factory().CreateLogger<Requisitor>();
    }
    
    public async Task<Tuple<HttpResponseMessage, long>> EnviarRequisicao(HttpRequestMessage requisicao, string nomeProvedorAlvo)
    {
        var watch = Stopwatch.StartNew();
        
        try
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(2);
            
            var resultado = await httpClient.SendAsync(requisicao);
            return new Tuple<HttpResponseMessage, long>(resultado, watch.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.Message);
            _logger.LogError("Erro ao enviar requisição para {NomeProvedor}", nomeProvedorAlvo);
            return new Tuple<HttpResponseMessage, long>(null, watch.ElapsedMilliseconds);
        }
    }
}
