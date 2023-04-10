namespace ApiBroker.API.Ranqueamento;

public class Ranqueador
{
    public async Task<List<string>> ObterOrdemMelhoresProvedores(string nomeRecurso)
    {
        using var influx = InfluxDbClientFactory.OpenConnection(); 
        var queryApi = influx.GetQueryApi();
        
        // todo: não utilizar nome do recurso direto na consulta (usar com @, como no Dapper)
        // todo: rever range na consulta (talvez ser parte da configuração do cliente)
        /*
         * todo: utilizando "logs-alt" por enquanto, mudar para "logs"
         *  Ajustar script para marcar errorCount como 0 caso a consulta não encontre
         *  nenhum registro em que sucesso._value = 0
         */
        var query = "ranking = () => {\n" +
                             "    meanLatency = from(bucket: \"logs-alt\")\n" +
                             "        |> range(start: -30d)\n" +
                             "        |> filter(fn: (r) => r[\"_measurement\"] == \"metricas_recursos\")\n" +
                             $"       |> filter(fn: (r) => r[\"nome_recurso\"] == \"{nomeRecurso}\")\n" +
                             "        |> filter(fn: (r) => r[\"_field\"] == \"latencia\")\n" +
                             "        |> mean()\n" +
                             "    errorCount = from(bucket: \"logs-alt\")\n" +
                             "        |> range(start: -30d)\n" +
                             "        |> filter(fn: (r) => r[\"_measurement\"] == \"metricas_recursos\")\n" +
                             "        |> filter(fn: (r) => r[\"_field\"] == \"sucesso\")\n" +
                             $"       |> filter(fn: (r) => r[\"nome_recurso\"] == \"{nomeRecurso}\")\n" +
                             "        |> filter(fn: (r) => r[\"_value\"] == 0)\n" +
                             "        |> count()\n" +
                             "    return join(tables: {meanLatency: meanLatency, errorCount: errorCount}, on: [\"nome_provedor\"])\n" +
                             "        |> map(fn: (r) => ({provider: r.nome_provedor, mean_latency: r._value_meanLatency, error_count: r._value_errorCount}))\n" +
                             "        |> sort(columns: [\"error_count\"])\n" +
                             "        |> yield(name: \"ranking\")\n" +
                             "}\n" +
                             "ranking()";

        var fluxTables = await queryApi.QueryAsync(query, "broker");

        var providers = (from fluxTable in fluxTables
            from fluxRecord in fluxTable.Records
            select new Dictionary<string, object>
            {
                { "name", fluxRecord.GetValueByKey("provider") },
                { "response_time", Convert.ToDouble(fluxRecord.GetValueByKey("mean_latency")) },
                { "error_rate", Convert.ToDouble(fluxRecord.GetValueByKey("error_count")) }
            }).ToList();

        return providers.Select(provider => (string)provider["name"]).ToList();
    }
}