using ReverseProxy.Forwarder;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
var configuration = builder.Configuration;

var app = builder.Build();

app.MapWhen(context => context.Request.Path.StartsWithSegments("/api"),
    applicationBuilder =>
    {
        applicationBuilder.Run(async context =>
        {
            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
            var handler = new ReverseProxyHandler(scopeFactory, context, configuration);
            await handler.Invoke();
        });
    });

app.Run();
