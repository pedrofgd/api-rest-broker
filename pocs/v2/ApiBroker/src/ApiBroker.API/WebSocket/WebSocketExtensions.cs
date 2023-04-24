namespace ApiBroker.API.WebSocket;

public static class WebSocketExtensions
{
    public static void AddWebSocket(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Portal", builder =>
            {
                builder.WithOrigins("http://localhost:3000")
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