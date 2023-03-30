namespace ApiBroker.API.Inicializacao;

public static class InicializacaoExtensions
{
    public static void UseInicializador(this WebApplication app, IConfiguration configuration)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var inicializador = new Inicializador();
            inicializador.Iniciar(configuration);
        });
    }
}