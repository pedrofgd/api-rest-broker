using System.Diagnostics;
using System.Net;
using Broker.API.Broker;
using Broker.API.Dados;
using Broker.API.Dados.Dtos;
using Broker.API.Mapeamento;
using Serilog;

namespace Broker.API.Validacao;

public class Validador
{
    private readonly SolicitacaoDto _recursoSettings;
    private readonly Criterios _criterios;
    private readonly MetricasRepository _metricasRepository;

    public Validador(SolicitacaoDto recursoSettings, MetricasRepository metricasRepository)
    {
        ArgumentNullException.ThrowIfNull(recursoSettings);
        _recursoSettings = recursoSettings;

        var criterios = MapearCriterios();
        ArgumentNullException.ThrowIfNull(criterios);
        _criterios = criterios;

        _metricasRepository = metricasRepository;
    }

    public bool Validar(RespostaMapeada respostaMapeada)
    {
        var watch = Stopwatch.StartNew();

        Log.Information("Validando resposta do provedor...");

        var criteriosAtingidos = 
            StatusCodeAceito(respostaMapeada.HttpResponseMessage.StatusCode) &&
            CompletudeAtingida(respostaMapeada.CamposMapeados);

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
        var statusCodeRecebidoAceito =
            _criterios.StatusHttpAceitos.Any(statusAceitos => statusAceitos == (int)statusCodeRecebido);

        if (!statusCodeRecebidoAceito)
            Log.Warning("O status code recebido ({StatusCode}) não é aceito", (int)statusCodeRecebido);

        return statusCodeRecebidoAceito;
    }

    private Criterios MapearCriterios()
    {
        var criteriosConfigurados = _recursoSettings.Criterios;
        return new Criterios
        {
            StatusHttpAceitos = criteriosConfigurados.StatusHttpAceitos,
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
        _metricasRepository.LogPerformanceCodigo(logDto);
    }
}