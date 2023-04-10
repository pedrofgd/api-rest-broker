using InfluxDB.Client;

namespace ApiBroker.API;

public class InfluxDbClientFactory
{
    /*
     * todo: substituir pelo Token gerado ao rodar o Influx localmente
     *  Depois colocar como variável ambiente..
     */
    private static readonly string Token = "YdbTQfAR79h6_yL-OzJOrnQ-2TYtm018z9tBlt5xP-HxdKlQg5qaictnkL7cry0d-1kG73QsRMHOQNlb1YJ1Dg==";

    public static InfluxDBClient OpenConnection()
    {
        return new InfluxDBClient("http://localhost:8086", Token);
    }
}