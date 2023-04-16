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

    public void Log(LogDto logDto)
    {
        // todo: ajustar para utilizar o InfluxDbClientFactory
        
        // todo: url do Influx como variável ambiente
        using var influx = InfluxDbClientFactory.OpenConnection();
        using var writeApi = influx.GetWriteApi();
        
        var point = PointData.Measurement("metricas_recursos")
            .Tag("nome_recurso", logDto.NomeRecurso)
            .Tag("nome_provedor", logDto.NomeProvedor)
            .Tag("origem", logDto.Origem)
            .Field("latencia", logDto.TempoRespostaMs)
            .Field("sucesso", logDto.Sucesso ? 1 : 0)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms);
        
        /*
         * todo: timeout aqui está passando batido...
         *  Quando o servidor do InfluxDB não está rodando, a aplicação tenta
         *  por alguns segundos e depois segue a execução, mesmo que não tenha gravado.
         *  Ver como lidar com erros e timeout
         */
        writeApi.WritePoint(point, "logs", "broker");
    }
}