namespace ApiBroker.API.Requisicao;

public class Requisitor
{
    public async Task<HttpResponseMessage> EnviarRequisicaoProvedor(HttpRequestMessage requisicao)
    {       
        var httpClient = new HttpClient();
        return await httpClient.SendAsync(requisicao);
    }
}