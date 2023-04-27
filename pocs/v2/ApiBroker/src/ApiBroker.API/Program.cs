using ApiBroker.API.Broker;
using ApiBroker.API.Inicializacao;
using ApiBroker.API.TestHelpers;
using ApiBroker.API.WebSocket;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddWebSocket();

#region Para testes durante a PoC

builder.Services.AddHealthCheckFake();

#endregion

if (builder.Environment.IsProduction())
{
    builder.WebHost.UseKestrel();
    builder.WebHost.ConfigureKestrel(opt => {
        opt.ListenAnyIP(5000);
    });
}

var app = builder.Build();

app.UseBroker();
app.UseInicializador(configuration);
app.UseWebSocket();

#region Para testes durante a PoC

app.UseProvedoresFake();
app.UseHealthcheckFake();

#endregion

app.Run();
