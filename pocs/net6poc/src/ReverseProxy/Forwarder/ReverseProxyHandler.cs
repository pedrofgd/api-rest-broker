using System.Diagnostics;

namespace ReverseProxy.Forwarder;

public class ReverseProxyHandler
{
  private readonly IServiceScopeFactory _serviceScopeFactory;
  private readonly HttpContext _httpContext;
  private readonly IConfiguration _configuration;

  public ReverseProxyHandler(IServiceScopeFactory serviceScopeFactory,
    HttpContext httpContext, IConfiguration configuration)
  {
    _serviceScopeFactory = serviceScopeFactory;
    _httpContext = httpContext;
    _configuration = configuration;
  }

  public async Task Invoke()
  {
    var watchProxyResponse = Stopwatch.StartNew();
    var scope = _serviceScopeFactory.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReverseProxyHandler>>();
    
    var targetResolver = new Resolver.TargetResolver(_configuration);
    targetResolver.BuildTargetUri(_httpContext.Request);
    
    using var responseMessage = await HttpForwarder.ForwardRequest(targetResolver, _httpContext);
    _httpContext.Response.StatusCode = (int)responseMessage.StatusCode;
    CopyFromTargetResponseHeaders(_httpContext, responseMessage);
    
    watchProxyResponse.Stop();
    logger.LogInformation("Total time elapsed for proxy to provider {TargetProvider}: {ElapsedMilliseconds}", 
      targetResolver.TargetProvider!.Name, watchProxyResponse.ElapsedMilliseconds);
    
    await responseMessage.Content.CopyToAsync(_httpContext.Response.Body);
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