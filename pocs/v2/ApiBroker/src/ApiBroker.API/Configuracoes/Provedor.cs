namespace ApiBroker.API.Configuracoes;

public class Provedor
{
    public string Nome { get; set; }
    public Uri Uri { get; set; }
    public List<CampoResposta> FormatoResposta { get; set; }
}