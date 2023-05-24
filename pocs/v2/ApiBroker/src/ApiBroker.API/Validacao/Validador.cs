using System.Diagnostics;
using System.Net;
using ApiBroker.API.Broker;
using ApiBroker.API.Dados;
using ApiBroker.API.Mapeamento;
using Serilog;

namespace ApiBroker.API.Validacao;

public class Validador
{
    private readonly SolicitacaoDto _recursoSettings;
    private readonly Criterios _criterios;
    private readonly MetricasDao _metricasDao;

    public Validador(SolicitacaoDto recursoSettings, MetricasDao metricasDao)
    {
        ArgumentNullException.ThrowIfNull(recursoSettings);
        _recursoSettings = recursoSettings;
        
        var criterios = MapearCriterios();
        ArgumentNullException.ThrowIfNull(criterios);
        _criterios = criterios;

        _metricasDao = metricasDao;
    }

    public bool Validar(RespostaMapeada respostaMapeada)
    {
        var watch = Stopwatch.StartNew();
        
        Log.Information("Validando resposta do provedor...");

        var criteriosAtingidos =
            CompletudeAtingida(respostaMapeada.CamposMapeados) &&
            StatusCodeAceito(respostaMapeada.HttpResponseMessage.StatusCode);

        Log.Information("Validação da resposta do provedor concluída. Resultado validação = {ResultadoValidacao}",
            criteriosAtingidos);
        
        LogPerformanceCodigo(watch.ElapsedMilliseconds);

        return criteriosAtingidos;
    }

    private bool CompletudeAtingida(Dictionary<string, string> camposRespostaProvedor)
    {
        if (camposRespostaProvedor is null) return false;
        
        foreach (var campoObrigatorio in _criterios.CamposObrigatorios)
        {
            var resposta = camposRespostaProvedor
                .SingleOrDefault(campo => 
                    campo.Key == campoObrigatorio);

            if (resposta.Key is null && resposta.Value is null)
            {
                Log.Warning("A resposta do provedor não trouxe todos os campos obrigatórios esperados");
                return false;
            }
        }

        return true;
    }

    private bool StatusCodeAceito(HttpStatusCode statusCodeRecebido)
    {
        var statusCodeRecebidoAceito = _criterios.StatusHttpAceitos.Any(statusAceitos => statusAceitos == (int)statusCodeRecebido);
        
        if (!statusCodeRecebidoAceito)
            Log.Warning("O status code recebido ({StatusCode}) não é aceito", (int)statusCodeRecebido);

        return statusCodeRecebidoAceito;
    }

    // todo: talvez isso precise ser movido para outro lugar... repensar quando implementar o Ranqueador
    private Criterios MapearCriterios()
    {
        var criteriosConfigurados = _recursoSettings.Criterios;
        return new Criterios
        {
            StatusHttpAceitos = criteriosConfigurados.StatusHttpAceitos,
            
            // todo: por enquanto considerando que todos são obrigatórios
            CamposObrigatorios = _recursoSettings.CamposResposta
        };
    }
    
    private void LogPerformanceCodigo(long tempoProcessamento)
    {
        var logDto = new LogPerformanceCodigoDto
        {
            NomeComponente = "Validador",
            TempoProcessamento = tempoProcessamento
        };
        _metricasDao.LogPerformanceCodigo(logDto);
    }
}