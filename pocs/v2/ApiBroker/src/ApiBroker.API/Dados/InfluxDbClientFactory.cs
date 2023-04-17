using InfluxDB.Client;

namespace ApiBroker.API;

public class InfluxDbClientFactory
{
    /*
     * todo: substituir pelo Token gerado ao rodar o Influx localmente
     *  Depois colocar como variável ambiente..
     */
    private static readonly string Token = "6KHxBBK6pQLe6yWw_GF9ECrjoyniCj_eDrlq7i0YoU-DGtbpuHmW8bL23dZj7CK6lhkWgp4P3yGGJEdgpDvX-Q==";

    public static InfluxDBClient OpenConnection()
    {
        return new InfluxDBClient("http://localhost:8086", Token);
    }
}