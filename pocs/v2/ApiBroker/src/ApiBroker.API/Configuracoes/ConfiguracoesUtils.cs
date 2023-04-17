namespace ApiBroker.API.Configuracoes;

public static class ConfiguracoesUtils
{
    /// <summary>
    /// Obtém todos os recursos configurados pelo Cliente
    /// </summary>
    /// <param name="configuration">Configurações definidas pelo cliente</param>
    /// <returns>Lista de todos os recursos configurados. Lista vazia se não houver nenhum</returns>
    public static List<RecursoSettings> ObterTodosRecursos(IConfiguration configuration)
    {
        var recursos = configuration.GetSection(RecursoSettings.RecursoConfig)
            .Get<List<RecursoSettings>>();

        if (recursos == null || !recursos.Any())
            return new List<RecursoSettings>();

        return recursos;
    }
    
    /// <summary>
    /// Obtém as configurações do recurso solicitado pelo nome
    /// </summary>
    /// <param name="nome">Nome configurado para o recurso</param>
    /// <param name="configuration">Configurações definidas pelo cliente</param>
    /// <returns>Recurso solicitado se existir</returns>
    public static RecursoSettings? ObterRecursoPeloNome(string nome, IConfiguration configuration)
    {
        var recursos = configuration.GetSection(RecursoSettings.RecursoConfig)
            .Get<List<RecursoSettings>>();
        
        return recursos?.FirstOrDefault(r => r.Nome == nome);
    }

    /// <summary>
    /// Obtém as configurações do provedor solicitado
    /// </summary>
    /// <param name="nomeRecurso">Nome do recurso a qual o provedor atende</param>
    /// <param name="nomeProvedor">Nome configurado para o provedor</param>
    /// <param name="configuration">Configurações definidas pelo cliente</param>
    /// <returns></returns>
    public static ProvedorSettings? ObterDadosProvedorRecurso(string nomeRecurso, string nomeProvedor, IConfiguration configuration)
    {
        var recursos = configuration.GetSection(RecursoSettings.RecursoConfig)
            .Get<List<RecursoSettings>>();
        
        var recurso = recursos?.FirstOrDefault(r => r.Nome == nomeRecurso);
        return recurso?.Provedores.FirstOrDefault(provedor => provedor.Nome == nomeProvedor);
    }
}