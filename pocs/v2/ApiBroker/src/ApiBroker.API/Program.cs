using ApiBroker.API.Broker;
using ApiBroker.API.Inicializacao;
using ApiBroker.API.TestHelpers;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Para testes na PoC
builder.Services.AddHealthCheckFake();

var app = builder.Build();

app.UseBroker();
// todo: check desabilitado durante os testes
app.UseInicializador(configuration, false);

// Para testes na PoC
app.UseProvedoresFake();
app.UseHealthcheckFake();

app.Run();
