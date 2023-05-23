using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Serilog;

namespace ApiBroker.API.Dados;

public class MetricasDao : IDisposable
{
    private readonly InfluxDBClient _influxDbClient;
    private readonly WriteApiAsync _writeApiAsync;

    // todo: mudar o nome, já que vai ficar só log
    public MetricasDao(IConfiguration configuration)
    {
        _influxDbClient = InfluxDbClientFactory.OpenConnection(configuration);
        _writeApiAsync = _influxDbClient.GetWriteApiAsync();
    }
    
    public void LogRespostaProvedor(LogRespostaProvedorDto logRespostaProvedorDto)
    {
        try
        {
            Log.Information("Registrando log de métricas da resposta do provedor {NomeRecurso}/{NomeProvedor}", 
                logRespostaProvedorDto.NomeRecurso, logRespostaProvedorDto.NomeProvedor);

            var point = PointData.Measurement("metricas_recursos")
                .Tag("nome_recurso", logRespostaProvedorDto.NomeRecurso)
                .Tag("nome_provedor", logRespostaProvedorDto.NomeProvedor)
                .Tag("origem", logRespostaProvedorDto.Origem)
                .Field("latencia", logRespostaProvedorDto.TempoRespostaMs)
                .Field("sucesso", logRespostaProvedorDto.Sucesso ? 1 : 0)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

            _writeApiAsync.WritePointAsync(point, "logs", "broker");
            
            Log.Information("Log de métricas da resposta do provedor registrado para a chamada no recurso {NomeRecurso}/{NomeProvedor}",
                logRespostaProvedorDto.NomeRecurso, logRespostaProvedorDto.NomeProvedor);
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
            Log.Information("Registrando log de performance do Broker para resposta a chamada no recurso {NomeRecurso}", 
                logPerformanceBrokerDto.NomeRecurso);

            var point = PointData.Measurement("performance_broker")
                .Tag("nome_recurso", logPerformanceBrokerDto.NomeRecurso)
                .Field("provedor_selecionado", logPerformanceBrokerDto.ProvedorSelecionado)
                .Field("tempo_resposta_total", logPerformanceBrokerDto.TempoRespostaTotal)
                .Field("tempo_resposta_provedor", logPerformanceBrokerDto.TempoRespostaProvedores)
                .Field("qtde_provedores_tentados", logPerformanceBrokerDto.QtdeProvedoresTentados)
                .Field("qtde_provedores_disponiveis", logPerformanceBrokerDto.QtdeProvedoresDisponiveis)
                .Field("retornou_erro_ao_cliente", logPerformanceBrokerDto.RetornouErroAoCliente)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

            _writeApiAsync.WritePointAsync(point, "logs", "broker");
            
            Log.Information("Log de performance do Broker registrado para a chamada no recurso {NomeRecurso}",
                logPerformanceBrokerDto.NomeRecurso);
        }
        catch (Exception e)
        {
            Log.Warning(
                "Erro ao registrar log de performance do Broker no InfluxDB para resposta a chamada no recurso {NomeRecurso}. " +
                "Erro: {MensagemErro}", logPerformanceBrokerDto.NomeRecurso, e.Message);
            throw;
        }
    }

    public void LogPerformanceCodigo(LogPerformanceCodigoDto logPerformanceCodigoDto)
    {
        try
        {
            Log.Debug("Registrando log de performance do código para o componente {NomeComponente}", 
                logPerformanceCodigoDto.NomeComponente);

            var point = PointData.Measurement("performance_codigo")
                .Tag("nome_componente", logPerformanceCodigoDto.NomeComponente)
                .Field("tempo_processamento", logPerformanceCodigoDto.TempoProcessamento)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

            _writeApiAsync.WritePointAsync(point, "logs", "broker");
            
            Log.Information("Log de performance do código registrado para o componente {NomeComponente}",
                logPerformanceCodigoDto.NomeComponente);
        }
        catch (Exception e)
        {
            Log.Warning(
                "Erro ao registrar log de performance do código no InfluxDB para o componente {NomeComponente}. " +
                "Erro: {MensagemErro}", logPerformanceCodigoDto.NomeComponente, e.Message);
            throw;
        }
    }
    
    public void LogAcompanhamentoProvedores(int qtdeProvedoresDisponiveis, string provedoresDisponiveis)
    {
        try
        {
            Log.Debug("Registrando log para acompanhamento dos provedores");

            var point = PointData.Measurement("acompanhamento_provedores")
                .Field("qtde_provedores_disponiveis", qtdeProvedoresDisponiveis)
                .Field("provedores_disponiveis", provedoresDisponiveis)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

            _writeApiAsync.WritePointAsync(point, "logs", "broker");
            
            Log.Information("Log para acompanhamento dos provedores registrado");
        }
        catch (Exception e)
        {
            Log.Warning(
                "Erro ao registrar log para acompanhamento dos provedores" +
                "Erro: {MensagemErro}", e.Message);
            throw;
        }
    }

    public void Dispose()
    {
        _influxDbClient?.Dispose();
        Log.Information("Conexão com o InfluxDB foi fechada");
    }
}