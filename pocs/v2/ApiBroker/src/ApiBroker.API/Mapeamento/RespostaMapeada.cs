namespace ApiBroker.API.Mapeamento;

public class RespostaMapeada
{
    public HttpResponseMessage HttpResponseMessage { get; set; }
    public Dictionary<string, string> CamposMapeados { get; set; }
}