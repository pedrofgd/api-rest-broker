using ApiBroker.API.Configuracoes;

namespace ApiBroker.API.Identificacao;

public class Identificador
{
    /// <summary>
    /// Identifica o recurso solicitado pelo cliente com base no nome enviado na requisição
    /// </summary>
    /// <param name="rota">Rota da requisição que o cliente fez no Broker</param>
    /// <param name="configuracoes">Configurações definidas pelo cliente</param>
    /// <returns>Recurso solicitado pelo cliente</returns>
    public SolicitacaoDto? IdentificarRecursoSolicitado(PathString rota, IConfiguration configuracoes)
    {
        if (!rota.StartsWithSegments("/api", out var infoRequisicao))
            return null;

        if (infoRequisicao.Value == null)
            return null;

        var infosRequisicao = infoRequisicao.Value.Split("/")[1..];
        
        var nomeRecurso = infosRequisicao[0];
        var recurso = ConfiguracoesUtils.ObterRecursoPeloNome(nomeRecurso, configuracoes);
        if (recurso is null)
            return null;

        var parametrosRotaEsperados = recurso.ParametrosViaRota;
        var parametrosRotaRecebidos = infosRequisicao[1..];
        var parametrosRotaMapeados = ObterParametrosRota(parametrosRotaEsperados, parametrosRotaRecebidos);
        if (parametrosRotaMapeados is null)
            return null;

        return new SolicitacaoDto
        {
            Nome = recurso.Nome,
            Provedores = recurso.Provedores,
            ParametrosRota = parametrosRotaMapeados,
            CamposResposta = recurso.CamposResposta,
            Criterios = recurso.Criterios
        };
    }

    private Dictionary<string, string>? ObterParametrosRota(IReadOnlyList<string> nomesParametrosRota, IReadOnlyList<string> valoresParametrosRota)
    {
        if (nomesParametrosRota.Count != valoresParametrosRota.Count)
            return null;

        var parametros = new Dictionary<string, string>();
        for (var i = 0; i < nomesParametrosRota.Count; i++)
        {
            var nome = nomesParametrosRota[i];
            var valor = valoresParametrosRota[i];
            parametros[nome] = valor;
        }

        return parametros;
    }
}