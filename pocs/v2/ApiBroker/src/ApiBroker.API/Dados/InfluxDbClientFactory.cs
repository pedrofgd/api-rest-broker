using ApiBroker.API.Configuracoes;
using InfluxDB.Client;

namespace ApiBroker.API.Dados;

public class InfluxDbClientFactory
{
    public static InfluxDBClient OpenConnection(IConfiguration configuration)
    {
        var config = ConfiguracoesUtils.ObterConfigInfluxDb(configuration);

        return new InfluxDBClient(
            config.Url,
            config.Token);
    }
}