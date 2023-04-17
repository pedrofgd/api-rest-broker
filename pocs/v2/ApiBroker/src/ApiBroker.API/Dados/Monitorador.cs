using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace ApiBroker.API.Monitoramento;

public class Monitorador
{
    public void Log(LogDto logDto)
    {
        using var influx = InfluxDbClientFactory.OpenConnection();
        var writeApi = influx.GetWriteApiAsync();

        var point = PointData.Measurement("metricas_recursos")
            .Tag("nome_recurso", logDto.NomeRecurso)
            .Tag("nome_provedor", logDto.NomeProvedor)
            .Tag("origem", logDto.Origem)
            .Field("latencia", logDto.TempoRespostaMs)
            .Field("sucesso", logDto.Sucesso ? 1 : 0)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms);
        
#pragma warning disable CS4014
        writeApi.WritePointAsync(point, "logs", "broker");
#pragma warning disable CS4014
    }
}