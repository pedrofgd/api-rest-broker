using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace ApiBroker.API.Dados;

public class MetricasDao
{
    private readonly ILogger<MetricasDao> _logger;

    public MetricasDao()
    {
        _logger = LoggerFactory.Factory().CreateLogger<MetricasDao>();
    }
    
    public void Log(LogDto logDto, IConfiguration configuration)
    {
        try
        {
            using var influx = InfluxDbClientFactory.OpenConnection(configuration);
            var writeApi = influx.GetWriteApi();
            
            _logger.LogInformation("Registrando log de {NomeRecurso}/{NomeProvedor}", 
                logDto.NomeRecurso, logDto.NomeProvedor);

            var point = PointData.Measurement("metricas_recursos")
                .Tag("nome_recurso", logDto.NomeRecurso)
                .Tag("nome_provedor", logDto.NomeProvedor)
                .Tag("origem", logDto.Origem)
                .Field("latencia", logDto.TempoRespostaMs)
                .Field("sucesso", logDto.Sucesso ? 1 : 0)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

            writeApi.WritePoint(point, "logs", "broker");
        }
        catch (Exception e)
        {
            _logger.LogWarning("Erro ao registrar log no InfluxDB. Erro: {MensagemErro}", e.Message);
            throw;
        }
    }

    public async Task<List<Dictionary<string, object>>> ObterDadosProvedores(String nomeRecurso, IConfiguration configuration)
    {
        try
        {
            _logger.LogInformation("Consultando base de monitoramento para obter os dados dos provedores");
            
            using var influx = InfluxDbClientFactory.OpenConnection(configuration);
            var queryApi = influx.GetQueryApi();
        
            var fluxTables = await queryApi.QueryAsync(Query(nomeRecurso), "broker");
        
            var provedores = (
                from fluxTable in fluxTables
                from fluxRecord in fluxTable.Records
                select new Dictionary<string, object>
                {
                    { "name", fluxRecord.GetValueByKey("provider") },
                    { "response_time", Convert.ToDouble(fluxRecord.GetValueByKey("mean_latency")) },
                    { "error_rate", Convert.ToInt32(fluxRecord.GetValueByKey("error_count")) }
                }).ToList();

            var listaProvedoresDisponiveis = provedores.Any()
                ? string.Join(",", provedores.Select(p => (string)p["name"]))
                : "0";

            _logger.LogInformation(
                "Base de monitoramento retornou os provedores para o recurso '{NomeRecurso}'. " +
                "Provedores: {ProvedoresDisponiveis}",
                nomeRecurso, listaProvedoresDisponiveis
            );
        
            return provedores;
        }
        catch (Exception)
        {
            _logger.LogWarning("Erro ao obter dados de provedores no InfluxDB");
            throw;
        }
    }

    private static string Query(string nomeRecurso)
    {
        // todo: rever range na consulta (talvez ser parte da configuração do cliente)
        return
            "ranking = () => {\n" +
            "    meanLatency = from(bucket: \"logs\")\n" +
            "        |> range(start: -5s)\n" + // todo: testando
            "        |> filter(fn: (r) => r[\"_measurement\"] == \"metricas_recursos\")\n" +
            $"       |> filter(fn: (r) => r[\"nome_recurso\"] == \"{nomeRecurso}\")\n" +
            "        |> filter(fn: (r) => r[\"_field\"] == \"latencia\")\n" +
            "        |> group(columns: [\"nome_provedor\"])\n" +
            "        |> mean()\n" +
            "    errorCount = from(bucket: \"logs\")\n" +
            "        |> range(start: -5s)\n" + // todo: testando
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