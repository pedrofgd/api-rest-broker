using System.Net;
using ApiBroker.API.Identificacao;
using ApiBroker.API.Mapeamento;

namespace ApiBroker.API.Validacao;

public class Validador
{
    private readonly SolicitacaoDto _recursoSettings;
    private readonly Criterios _criterios;

    public Validador(SolicitacaoDto recursoSettings)
    {
        ArgumentNullException.ThrowIfNull(recursoSettings);
        _recursoSettings = recursoSettings;
        
        var criterios = MapearCriterios();
        ArgumentNullException.ThrowIfNull(criterios);
        _criterios = criterios;
    }

    public bool Validar(RespostaMapeada respostaMapeada)
    {
        return !CompletudeFalhou(respostaMapeada.CamposMapeados) &&
               !StatusCodeNaoAceito(respostaMapeada.HttpResponseMessage.StatusCode);
    }

    private bool CompletudeFalhou(Dictionary<string, string> camposRespostaProvedor)
    {
        return _criterios.CamposObrigatorios.Select(camposRespostaProvedor.GetValueOrDefault)
            .Any(retornoProvedor => retornoProvedor is null);
    }

    private bool StatusCodeNaoAceito(HttpStatusCode statusCode)
    {
        int? statusAceito = _criterios.StatusHttpAceitos
            .FirstOrDefault(x => x == (int)statusCode);
        return statusAceito == null;
    }

    // todo: talvez isso precise ser movido para outro lugar... repensar quando implementar o Ranqueador
    private Criterios MapearCriterios()
    {
        var criteriosConfigurados = _recursoSettings.Criterios;
        return new Criterios
        {
            RequisicoesEstimadasHora = criteriosConfigurados.RequisicoesEstimadasHora,
            P99LatenciaMs = criteriosConfigurados.P99LatenciaMs,
            P95LatenciaMs = criteriosConfigurados.P95LatenciaMs,
            ErrorBudgetHora = criteriosConfigurados.ErrorBudgetHora,
            StatusHttpAceitos = criteriosConfigurados.StatusHttpAceitos,
            
            // todo: por enquanto considerando que todos são obrigatórios
            CamposObrigatorios = _recursoSettings.CamposResposta
        };
    }
}