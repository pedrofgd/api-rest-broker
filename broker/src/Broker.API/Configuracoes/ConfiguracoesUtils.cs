namespace Broker.API.Configuracoes;

public static class ConfiguracoesUtils
{
    public static BrokerSettings ObterConfiguracoesBroker(IConfiguration configuration)
    {
        return configuration.GetSection(BrokerSettings.BrokerConfig)
            .Get<BrokerSettings>();
    }
    
    public static List<RecursoSettings> ObterTodosRecursos(IConfiguration configuration)
    {
        var recursos = configuration.GetSection(RecursoSettings.RecursoConfig)
            .Get<List<RecursoSettings>>();

        if (recursos == null || !recursos.Any())
            return new List<RecursoSettings>();

        return recursos;
    }

    public static RecursoSettings ObterRecursoPeloNome(string nome, IConfiguration configuration)
    {
        var recursos = configuration.GetSection(RecursoSettings.RecursoConfig)
            .Get<List<RecursoSettings>>();

        return recursos?.FirstOrDefault(r => r.Nome == nome);
    }

    public static ProvedorSettings ObterDadosProvedorRecurso(string nomeRecurso, string nomeProvedor,
        IConfiguration configuration)
    {
        var recursos = configuration.GetSection(RecursoSettings.RecursoConfig)
            .Get<List<RecursoSettings>>();

        var recurso = recursos?.FirstOrDefault(r => r.Nome == nomeRecurso);
        return recurso?.Provedores.FirstOrDefault(provedor => provedor.Nome == nomeProvedor);
    }

    public static InfluxDbSettings ObterConfigInfluxDb(IConfiguration configuration)
    {
        return configuration.GetSection(InfluxDbSettings.InfluxDbConfig)
            .Get<InfluxDbSettings>();
    }
}