using Broker.API.Configuracoes;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Broker.API.Broker;

public class SolicitacaoDto
{
    public string NomeRecurso { get; set; }
    public string[] CamposResposta { get; set; }
    public Dictionary<string, string> ParametrosRota { get; set; }
    public CriteriosSettings Criterios { get; set; }
    public bool TentarTodosProvedoresAteSucesso { get; set; }
}