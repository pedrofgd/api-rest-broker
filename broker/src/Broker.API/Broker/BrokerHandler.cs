namespace Broker.API.Broker;

public class BrokerHandler
{
    private readonly RequestDelegate _next;

    public BrokerHandler(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, Orquestrador orquestrador)
    {
        await orquestrador.Orquestrar(context);
    }
}