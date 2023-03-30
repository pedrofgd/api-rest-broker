using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace ApiBroker.API.Monitoramento;

public class Monitorador
{
    /*
     * todo: substituir pelo Token gerado ao rodar o Influx localmente
     *  Depois colocar como variável ambiente..
     */
    private static readonly string Token = "YdbTQfAR79h6_yL-OzJOrnQ-2TYtm018z9tBlt5xP-HxdKlQg5qaictnkL7cry0d-1kG73QsRMHOQNlb1YJ1Dg==";
 
    public void Log(LogDto logDto)
    {
        // todo: url do Influx como variável ambiente
        using var influx = new InfluxDBClient("http://localhost:8086", Token);
        using var writeApi = influx.GetWriteApi();
        
        var point = PointData.Measurement("metricas_recursos")
            .Tag("nome_recurso", logDto.NomeRecurso)
            .Tag("nome_provedor", logDto.NomeProvedor)
            .Tag("origem", logDto.Origem)
            .Field("latencia", logDto.TempoRespostaMs)
            .Field("sucesso", logDto.Sucesso) // todo: avaliar enviar como 0 ou 1
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
        
        writeApi.WritePoint(point, "logs", "broker");
    }
}