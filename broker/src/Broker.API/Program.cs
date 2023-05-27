using Broker.API.Broker;
using Broker.API.Dados;
using Broker.API.Inicializacao;
using Broker.API.Mapeamento;
using Broker.API.Ranqueamento;
using Broker.API.Requisicao;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.UseSerilog((context, config) => 
    config.ReadFrom.Configuration(context.Configuration));

Log.Information("Iniciando Broker...");

builder.Services.AddControllers();
builder.Services.AddRedis(configuration);
builder.Services.AddScoped<Orquestrador>();
builder.Services.AddScoped<Ranqueador>();
builder.Services.AddScoped<Mapeador>();
builder.Services.AddScoped<Requisitor>();
builder.Services.AddSingleton<MetricasRepository>();
builder.Services.AddHttpClient("Requisitor");

var app = builder.Build();
app.UseInicializador(configuration, app.Services.GetRequiredService<IServiceScopeFactory>());
app.UseBroker(configuration);
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();

Log.Information("Broker iniciado com sucesso");

app.Run();