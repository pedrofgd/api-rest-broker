namespace Broker.API.Configuracoes;

public class InfluxDbSettings
{
    public const string InfluxDbConfig = "InfluxDbSettings";
    public string Url { get; set; }
    public string Token { get; set; }
    public string BucketPadrao { get; set; }
    public string OrganizacaoPadrao { get; set; }
}