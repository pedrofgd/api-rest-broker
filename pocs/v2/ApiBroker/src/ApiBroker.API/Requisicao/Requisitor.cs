using System.Diagnostics;

namespace ApiBroker.API.Requisicao;

public class Requisitor
{
    public async Task<Tuple<HttpResponseMessage, long>> EnviarRequisicaoProvedor(HttpRequestMessage requisicao)
    {
        var watch = Stopwatch.StartNew();
        var httpClient = new HttpClient();
        var resultado = await httpClient.SendAsync(requisicao); 
        return new Tuple<HttpResponseMessage, long>(resultado, watch.ElapsedMilliseconds);
    }
}
