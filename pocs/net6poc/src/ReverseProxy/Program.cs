using Microsoft.Extensions.Caching.Distributed;
using ReverseProxy;
using ReverseProxy.Forwarder;
using ReverseProxy.Redis;
using ReverseProxy.Resolver;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
var configuration = builder.Configuration;

// Configure services
builder.Services.AddSingleton<TargetResolver>();

builder.Services.AddRedis(configuration);

builder.Services.AddSingleton(provider => ConnectionMultiplexer.Connect("localhost"));
builder.Services.AddSingleton<IDistributedCache, RedisCache>();

var app = builder.Build();
app.Lifetime.ApplicationStopped.Register(RedisExtensions.CleanUp);

app.MapWhen(context => context.Request.Path.StartsWithSegments("/api"),
    applicationBuilder =>
    {
        applicationBuilder.UseMiddleware<ReverseProxyHandler>();
    });

app.Run();
