using Broker.API.Configuracoes;
using InfluxDB.Client;

namespace Broker.API.Dados;

public class InfluxDbClientFactory
{
    public static InfluxDBClient OpenConnection(InfluxDbSettings settings)
    {
        return new InfluxDBClient(settings.Url, settings.Token);
    }
}