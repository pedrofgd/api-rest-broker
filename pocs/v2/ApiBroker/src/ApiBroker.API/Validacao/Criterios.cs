namespace ApiBroker.API.Validacao;

public class Criterios
{
    public int RequisicoesEstimadasHora { get; set; }
    public int P99LatenciaMs { get; set; }
    public int P95LatenciaMs { get; set; }
    public int ErrorBudgetHora { get; set; }
    public int[] StatusHttpAceitos { get; set; }
    public string[] CamposObrigatorios { get; set; }
}