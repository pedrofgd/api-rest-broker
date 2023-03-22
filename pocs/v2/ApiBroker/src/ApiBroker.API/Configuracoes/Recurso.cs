namespace ApiBroker.API.Configuracoes;

public class Recurso
{
    public string Nome { get; set; }
    public List<Provedor> Provedores { get; set; }
    public string[] CamposResposta { get; set; }
}