using ApiBroker.API.Broker;
using ApiBroker.API.Dados;
using ApiBroker.API.Inicializacao;
using ApiBroker.API.Ranqueamento;
using ApiBroker.API.Requisicao;
using ApiBroker.API.TestHelpers;
using ApiBroker.API.WebSocket;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

Log.Information("Iniciando Broker...");

builder.Services.AddScoped<Orquestrador>();
builder.Services.AddScoped<MetricasDao>();
builder.Services.AddScoped<Requisitor>();
builder.Services.AddScoped<Ranqueador>();
builder.Services.AddWebSocket(configuration);

builder.Services.AddHttpClient("Requisitor");

#region Para testes durante a PoC

builder.Services.AddHealthCheckFake();

#endregion

var app = builder.Build();

app.UseBroker();
app.UseInicializador(configuration, app.Services.GetRequiredService<IServiceScopeFactory>());
app.UseWebSocket();

#region Para testes durante a PoC

app.UseProvedoresFake();
app.UseHealthcheckFake();

#endregion

app.UseHttpsRedirection();

app.Run();
