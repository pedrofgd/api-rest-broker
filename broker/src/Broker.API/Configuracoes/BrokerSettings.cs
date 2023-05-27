namespace Broker.API.Configuracoes;

public class BrokerSettings
{
    public const string BrokerConfig = "BrokerSettings";
    public string RotaApiPadrao { get; set; }
    public bool GravarLogsPerformance { get; set; }
}