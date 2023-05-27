namespace Broker.API.Mapeamento;

public class RespostaMapeada
{
    public HttpResponseMessage HttpResponseMessage { get; init; }
    public Dictionary<string, string> CamposMapeados { get; init; }
}