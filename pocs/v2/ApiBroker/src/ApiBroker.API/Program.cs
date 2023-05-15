using ApiBroker.API.Broker;
using ApiBroker.API.Dados;
using ApiBroker.API.Inicializacao;
using ApiBroker.API.TestHelpers;
using ApiBroker.API.WebSocket;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddScoped<Orquestrador>();
builder.Services.AddScoped<MetricasDao>();
builder.Services.AddWebSocket(configuration);

#region Para testes durante a PoC

builder.Services.AddHealthCheckFake();

#endregion

var app = builder.Build();

app.UseBroker();
app.UseInicializador(configuration);
app.UseWebSocket();

#region Para testes durante a PoC

app.UseProvedoresFake();
app.UseHealthcheckFake();

#endregion

app.UseHttpsRedirection();

app.Run();
