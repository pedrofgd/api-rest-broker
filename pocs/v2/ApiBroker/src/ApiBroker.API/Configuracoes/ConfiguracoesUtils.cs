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
}