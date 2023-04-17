// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ApiBroker.API.Configuracoes;

public class CriteriosSettings
{
    public int RequisicoesEstimadasHora { get; set; }
    public int P99LatenciaMs { get; set; }
    public int P95LatenciaMs { get; set; }
    public int ErrorBudgetHora { get; set; }
    public int[] StatusHttpAceitos { get; set; }
}