using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = int.Parse(configuration["PermitLimitApiCep"]!);
        options.Window = TimeSpan.FromSeconds(int.Parse(configuration["WindowFromSecondsLimitApiCep"]!));
        options.QueueLimit = 0;
    }));

var app = builder.Build();

app.UseRateLimiter();

app.MapPost("/correios-alt/{cep}", (string cep) => Results.Ok(new
{
    erro = false,
    mensagem = Guid.NewGuid().ToString(),
    total = 1,
    dados = new
    {
        uf = Guid.NewGuid().ToString(),
        localidade = Guid.NewGuid().ToString(),
        locNoSem = "",
        locNu = Guid.NewGuid().ToString(),
        localidadeSubordinada = "",
        logradouroDNEC = Guid.NewGuid().ToString(),
        logradouroTextoAdicional = "",
        logradouroTexto = "",
        bairro = Guid.NewGuid().ToString(),
        baiNu = "",
        nomeUnidade = "",
        cep,
        tipoCep = Guid.NewGuid().ToString(),
        numeroLocalidade = Guid.NewGuid().ToString(),
        situacao = "",
        faixasCaixaPostal = Guid.NewGuid().ToString(),
        faixasCep = Guid.NewGuid().ToString(),
    }
})).RequireRateLimiting("fixed");

app.MapGet("/viacep/{cep}", (string cep) => Results.Ok(new
{
    cep,
    logradouro = Guid.NewGuid().ToString(),
    complemento = Guid.NewGuid().ToString(),
    bairro = Guid.NewGuid().ToString(),
    localidade = Guid.NewGuid().ToString(),
    uf = Guid.NewGuid().ToString(),
    ibge = Guid.NewGuid().ToString(),
    gia = Guid.NewGuid().ToString(),
    ddd = Guid.NewGuid().ToString(),
    siafi = Guid.NewGuid().ToString()
})).RequireRateLimiting("fixed");

app.MapGet("/widenet/{cep}", (string cep) => Results.Ok(new
{
    code = Guid.NewGuid().ToString(),
    state = Guid.NewGuid().ToString(),
    city = Guid.NewGuid().ToString(),
    district = Guid.NewGuid().ToString(),
    address = Guid.NewGuid().ToString(),
    cep
})).RequireRateLimiting("fixed");

app.Run();