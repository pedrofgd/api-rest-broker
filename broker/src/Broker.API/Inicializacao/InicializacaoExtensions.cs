namespace Broker.API.Inicializacao;

public static class InicializacaoExtensions
{
    public static void UseInicializador(this WebApplication app, IConfiguration configuration, 
        IServiceScopeFactory serviceScopeFactory, bool check = true)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var inicializador = new Inicializador(configuration, serviceScopeFactory);
            inicializador.Iniciar(check);
        });
    }
}