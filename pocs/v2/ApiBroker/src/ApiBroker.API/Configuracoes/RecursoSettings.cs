namespace ApiBroker.API.Configuracoes;

public class RecursoSettings
{
    public const string RecursoConfig = "Recursos";
    public string Nome { get; set; }
    public string[] ParametrosViaRota { get; set; }
    public string[] CamposResposta { get; set; }
    public List<ProvedorSettings> Provedores { get; set; }
}