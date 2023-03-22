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
    public Recurso? IdentificarRecursoSolicitado(PathString rota, IConfiguration configuracoes)
    {
        if (!rota.StartsWithSegments("/api", out var infoRequisicao))
            return null;

        if (infoRequisicao.Value == null)
            return null;

        var infosRequisicao = infoRequisicao.Value.Split("/")[1..];
        var nomeRecurso = infosRequisicao[0];
        return ObterRecurso(nomeRecurso, configuracoes);
    }

    /// <summary>
    /// Obtém as configurações do recurso solicitado
    /// </summary>
    /// <param name="nome">Nome configurado para o recurso</param>
    /// <param name="configuration">Configurações definidas pelo cliente</param>
    /// <returns>Recurso solicitado se existir</returns>
    private Recurso? ObterRecurso(string nome, IConfiguration configuration)
    {
        var recursos = configuration.Get<List<Recurso>>();
        return recursos?.FirstOrDefault(r => r.Nome == nome);
    }
}