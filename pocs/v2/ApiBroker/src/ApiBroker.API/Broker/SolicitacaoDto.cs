using ApiBroker.API.Configuracoes;

namespace ApiBroker.API.Broker;

public class SolicitacaoDto
{
    public string Nome { get; set; }
    public List<ProvedorSettings> Provedores { get; set; }
    public string[] CamposResposta { get; set; }
    public Dictionary<string, string> ParametrosRota { get; set; }
    public CriteriosSettings Criterios { get; set; }
    public bool TentarTodosProvedoresAteSucesso { get; set; }
}