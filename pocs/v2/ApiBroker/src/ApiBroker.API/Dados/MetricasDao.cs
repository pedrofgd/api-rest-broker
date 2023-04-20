using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace ApiBroker.API.Dados;

public class MetricasDao
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

    public async Task<List<Dictionary<string, object>>> ObterDadosProvedores(String nomeRecurso)
    {
        using var influx = InfluxDbClientFactory.OpenConnection();
        var queryApi = influx.GetQueryApi();
        
        var fluxTables = await queryApi.QueryAsync(Query(nomeRecurso), "broker");
        
        var providers = (
            from fluxTable in fluxTables
            from fluxRecord in fluxTable.Records
            select new Dictionary<string, object>
            {
                { "name", fluxRecord.GetValueByKey("provider") },
                { "response_time", Convert.ToDouble(fluxRecord.GetValueByKey("mean_latency")) },
                { "error_rate", Convert.ToInt32(fluxRecord.GetValueByKey("error_count")) }
            }).ToList();
        
        return providers;
    }

    private static string Query(string nomeRecurso)
    {
        // todo: rever range na consulta (talvez ser parte da configuração do cliente)
        return
            "ranking = () => {\n" +
            "    meanLatency = from(bucket: \"logs\")\n" +
            "        |> range(start: -1h)\n" +
            "        |> filter(fn: (r) => r[\"_measurement\"] == \"metricas_recursos\")\n" +
            $"       |> filter(fn: (r) => r[\"nome_recurso\"] == \"{nomeRecurso}\")\n" +
            "        |> filter(fn: (r) => r[\"_field\"] == \"latencia\")\n" +
            "        |> group(columns: [\"nome_provedor\"])\n" +
            "        |> mean()\n" +
            "    errorCount = from(bucket: \"logs\")\n" +
            "        |> range(start: -1h)\n" +
            "        |> filter(fn: (r) => r[\"_measurement\"] == \"metricas_recursos\")\n" +
            "        |> filter(fn: (r) => r[\"_field\"] == \"sucesso\")\n" +
            $"       |> filter(fn: (r) => r[\"nome_recurso\"] == \"{nomeRecurso}\")\n" +
            "        |> filter(fn: (r) => r[\"_value\"] == 0, onEmpty: \"keep\")\n" +
            "        |> group(columns: [\"nome_provedor\"])\n" +
            "        |> count()\n" +
            "    return join(tables: {meanLatency: meanLatency, errorCount: errorCount}, on: [\"nome_provedor\"])\n" +
            "        |> map(fn: (r) => ({provider: r.nome_provedor, mean_latency: r._value_meanLatency, " +
            "               error_count: r._value_errorCount}))\n" +
            "        |> sort(columns: [\"error_count\"])\n" +
            "        |> yield(name: \"ranking\")\n" +
            "}\n" +
            "ranking()";
    }
}