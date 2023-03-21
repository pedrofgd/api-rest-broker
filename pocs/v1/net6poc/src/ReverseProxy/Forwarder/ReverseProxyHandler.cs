using System.Diagnostics;
using ReverseProxy.Resolver;

namespace ReverseProxy.Forwarder;

public class ReverseProxyHandler
{
  private readonly ILogger<ReverseProxyHandler> _logger;
  private readonly TargetResolver _targetResolver;

  public ReverseProxyHandler(
    RequestDelegate next,
    ILogger<ReverseProxyHandler> logger,
    TargetResolver targetResolver)
  {
    _logger = logger;
    _targetResolver = targetResolver;
  }

  public async Task Invoke(HttpContext context)
  {
    var watchProxyResponse = Stopwatch.StartNew();

    var target = _targetResolver.ResolveTarget(context.Request);
    
    using var responseMessage = await HttpForwarder.ForwardRequest(target, context);
    context.Response.StatusCode = (int)responseMessage.StatusCode;
    CopyFromTargetResponseHeaders(context, responseMessage);
    
    watchProxyResponse.Stop();
    _logger.LogInformation("Total time elapsed for proxy to provider {TargetProvider}: {ElapsedMilliseconds}", 
      target.TargetProvider.Name, watchProxyResponse.ElapsedMilliseconds);
    
    await responseMessage.Content.CopyToAsync(context.Response.Body);
  }

  private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
  {
    foreach (var header in responseMessage.Headers)
    {
      context.Response.Headers[header.Key] = header.Value.ToArray();
    }

    foreach (var header in responseMessage.Content.Headers)
    {
      context.Response.Headers[header.Key] = header.Value.ToArray();
    }
    context.Response.Headers.Remove("transfer-encoding");
  }
}