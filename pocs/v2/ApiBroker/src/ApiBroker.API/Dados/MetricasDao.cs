using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Serilog;

namespace ApiBroker.API.Dados;

public class MetricasDao : IDisposable
{
    private readonly InfluxDBClient _influxDbClient;
    
    public MetricasDao(IConfiguration configuration)
    {
        _influxDbClient = InfluxDbClientFactory.OpenConnection(configuration);
    }
    
    public void LogRespostaProvedor(LogRespostaProvedorDto logRespostaProvedorDto)
    {
        try
        {
            var writeApi = _influxDbClient.GetWriteApiAsync();
            
            Log.Information("Registrando log da resposta do provedor {NomeRecurso}/{NomeProvedor}", 
                logRespostaProvedorDto.NomeRecurso, logRespostaProvedorDto.NomeProvedor);

            var point = PointData.Measurement("metricas_recursos")
                .Tag("nome_recurso", logRespostaProvedorDto.NomeRecurso)
                .Tag("nome_provedor", logRespostaProvedorDto.NomeProvedor)
                .Tag("origem", logRespostaProvedorDto.Origem)
                .Field("latencia", logRespostaProvedorDto.TempoRespostaMs)
                .Field("sucesso", logRespostaProvedorDto.Sucesso ? 1 : 0)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

            writeApi.WritePointAsync(point, "logs", "broker");
        }
        catch (Exception e)
        {
            Log.Warning("Erro ao registrar log da resposta do provedor no InfluxDB. Erro: {MensagemErro}", e.Message);
            throw;
        }
    }

    public void LogPerformanceBroker(LogPerformanceBrokerDto logPerformanceBrokerDto)
    {
        try
        {
            var writeApi = _influxDbClient.GetWriteApiAsync();
            
            Log.Information("Registrando log de performance do Broker para resposta a chamada no recurso {NomeRecurso}", 
                logPerformanceBrokerDto.NomeRecurso);

            var point = PointData.Measurement("performance_broker")
                .Tag("nome_recurso", logPerformanceBrokerDto.NomeRecurso)
                .Field("provedor_selecionado", logPerformanceBrokerDto.ProvedorSelecionado)
                .Field("tempo_resposta_total", logPerformanceBrokerDto.TempoRespostaTotal)
                .Field("tempo_resposta_provedor", logPerformanceBrokerDto.TempoRespostaProvedores)
                .Field("qtde_provedores_tentados", logPerformanceBrokerDto.QtdeProvedoresTentados)
                .Field("retornou_erro_ao_cliente", logPerformanceBrokerDto.RetornouErroAoCliente)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

            writeApi.WritePointAsync(point, "logs", "broker");
        }
        catch (Exception e)
        {
            Log.Warning(
                "Erro ao registrar log de performance do Broker no InfluxDB para resposta a chamada no recurso {NomeRecurso}. " +
                "Erro: {MensagemErro}", logPerformanceBrokerDto.NomeRecurso, e.Message);
            throw;
        }
    }

    public async Task<List<Dictionary<string, object>>> ObterDadosProvedores(string nomeRecurso)
    {
        try
        {
            Log.Information("Consultando base de monitoramento para obter os dados dos provedores");
            
            var queryApi = _influxDbClient.GetQueryApi();
            
            // todo: rever range na consulta (talvez ser parte da configuração do cliente)
            var query =
                "ranking = () => {\n" +
                "    meanLatency = from(bucket: \"logs\")\n" +
                "        |> range(start: -5m)\n" + // todo: testando
                "        |> filter(fn: (r) => r[\"_measurement\"] == \"metricas_recursos\")\n" +
                $"       |> filter(fn: (r) => r[\"nome_recurso\"] == \"{nomeRecurso}\")\n" +
                "        |> filter(fn: (r) => r[\"_field\"] == \"latencia\")\n" +
                "        |> group(columns: [\"nome_provedor\"])\n" +
                "        |> mean()\n" +
                "    errorCount = from(bucket: \"logs\")\n" +
                "        |> range(start: -5m)\n" + // todo: testando
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
        
            var fluxTables = await queryApi.QueryAsync(query, "broker");
        
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

            Log.Information(
                "Base de monitoramento retornou os provedores para o recurso '{NomeRecurso}'. " +
                "Provedores: {ProvedoresDisponiveis}",
                nomeRecurso, listaProvedoresDisponiveis
            );
        
            return provedores;
        }
        catch (Exception)
        {
            Log.Warning("Erro ao obter dados de provedores no InfluxDB");
            throw;
        }
    }

    public void Dispose()
    {
        _influxDbClient?.Dispose();
    }
}