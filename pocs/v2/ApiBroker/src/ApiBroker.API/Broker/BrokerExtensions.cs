namespace ApiBroker.API.Broker;

public static class BrokerExtensions
{
    /// <summary>
    /// Configura a rota padrão do Broker em /api e
    /// e define o handler para processar a requisição
    /// </summary>
    /// <param name="app">WebApplication do Startup</param>
    public static void UseBroker(this WebApplication app)
    {
        app.MapWhen(
            context => context.Request.Path.StartsWithSegments("/api"),
            builder => { builder.UseMiddleware<BrokerHandler>(); }
        );
    }
}