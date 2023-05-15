using System.Diagnostics;
using System.Net;
using ApiBroker.API.Configuracoes;
using ApiBroker.API.Mapeamento;
using ApiBroker.API.Requisicao;
using ApiBroker.API.Dados;
using ApiBroker.API.Ranqueamento;
using ApiBroker.API.Validacao;
using ApiBroker.API.WebSocket;
using Microsoft.AspNetCore.SignalR;

namespace ApiBroker.API.Broker;

public class BrokerHandler
{
    private readonly RequestDelegate _next;

    public BrokerHandler(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Método invocado para manipular a requisição quando
    /// o cliente chama o Broker no endpoint configurado
    /// </summary>
    public async Task Invoke(HttpContext context, Orquestrador orquestrador)
    {
        await orquestrador.Orquestrar(context);
    }
}