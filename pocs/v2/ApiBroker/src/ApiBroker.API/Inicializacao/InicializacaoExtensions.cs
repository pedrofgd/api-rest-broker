namespace ApiBroker.API.Inicializacao;

public static class InicializacaoExtensions
{
    public static void UseInicializador(this WebApplication app, IConfiguration configuration, 
        bool check = true)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var inicializador = new Inicializador(configuration);
            inicializador.Iniciar(check);
        });
    }
}