namespace ApiBroker.API.TestHelpers;

public static class HealthcheckFakeExtensions
{
    public static void AddHealthCheckFake(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<HealthcheckFakeHandler>("fake");
    }
    
    public static void UseHealthcheckFake(this WebApplication app)
    {
        app.MapHealthChecks("/healthz");
    }
}