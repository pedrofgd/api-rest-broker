using Broker.API.Configuracoes;

namespace Broker.API.Broker;

public static class BrokerExtensions
{
    public static void UseBroker(this WebApplication app, IConfiguration configuration)
    {
        var configuracoesBroker = ConfiguracoesUtils.ObterConfiguracoesBroker(configuration);
        ArgumentNullException.ThrowIfNull(configuracoesBroker);
        var rotaApiPadrao = configuracoesBroker.RotaApiPadrao;
        ArgumentException.ThrowIfNullOrEmpty(rotaApiPadrao);

        app.MapWhen(
            context => context.Request.Path.StartsWithSegments(rotaApiPadrao),
            builder => { builder.UseMiddleware<BrokerHandler>(); }
        );
    }
}