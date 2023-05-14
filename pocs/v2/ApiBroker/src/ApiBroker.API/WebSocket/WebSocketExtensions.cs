namespace ApiBroker.API.WebSocket;

public static class WebSocketExtensions
{
    public static void AddWebSocket(this IServiceCollection services, IConfiguration configuration)
    {
        var hostPortal = configuration["PortalSettings:Host"];
        ArgumentException.ThrowIfNullOrEmpty(hostPortal);
        
        services.AddCors(options =>
        {
            options.AddPolicy("Portal", builder =>
            {
                builder.WithOrigins(hostPortal)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        
        services.AddSignalR();
    }

    public static void UseWebSocket(this WebApplication app)
    {
        app.UseCors("Portal");
        app.MapHub<RanqueamentoHub>("/ranqueamento").RequireCors("Portal");
    }
}