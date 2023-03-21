using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using ReverseProxy.Model;
using ReverseProxy.Resolver;
using Serilog;

namespace ReverseProxy.Forwarder;

public static class HttpForwarder
{
    private static readonly HttpClient HttpClient = new();
    
    public static async Task<HttpResponseMessage> ForwardRequest(ProviderResolved resolver, HttpContext context)
    {
        var watchProviderResponse = Stopwatch.StartNew();
        
        var targetProvider = resolver.TargetProvider;
        var requestMessage = CreateTargetMessage(context, resolver.TargetUri);

        var providerResponse = await HttpClient.SendAsync(requestMessage,
            HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
        watchProviderResponse.Stop();
        
        var mappedResponse = MapResponseBackToClient(providerResponse, targetProvider, resolver.Resource);

        watchProviderResponse.Stop();
        Log.Logger.Information("Time elapsed for request: {ElapsedMilliseconds}", watchProviderResponse.ElapsedMilliseconds);

        return mappedResponse;
    }
    
    private static HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
    {
        var requestMessage = new HttpRequestMessage();
        CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

        requestMessage.RequestUri = targetUri;
        requestMessage.Headers.Host = targetUri.Host;
        requestMessage.Method = GetMethod(context.Request.Method);

        return requestMessage;
    }
    
    private static HttpMethod GetMethod(string method)
    {
        if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
        if (HttpMethods.IsGet(method)) return HttpMethod.Get;
        if (HttpMethods.IsHead(method)) return HttpMethod.Head;
        if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
        if (HttpMethods.IsPost(method)) return HttpMethod.Post;
        if (HttpMethods.IsPut(method)) return HttpMethod.Put;
        if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
        return new HttpMethod(method);
    }

    private static void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
    {
        var requestMethod = context.Request.Method;

        if (!HttpMethods.IsGet(requestMethod) &&
            !HttpMethods.IsHead(requestMethod) &&
            !HttpMethods.IsDelete(requestMethod) &&
            !HttpMethods.IsTrace(requestMethod))
        {
            var streamContent = new StreamContent(context.Request.Body);
            requestMessage.Content = streamContent;
        }

        foreach (var header in context.Request.Headers)
        {
            requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    private static HttpResponseMessage MapResponseBackToClient(HttpResponseMessage originalResponse, Provider provider, Resource resource)
    {
        var response = new HttpResponseMessage();

        var responseBody = originalResponse.Content.ReadAsStringAsync().Result;
        var mappedResponse = ReplaceResponseFields(responseBody, provider);
        var filteredResponse = GetResponseWithSelectedFields(mappedResponse, resource, provider.Name);
        
        var newContentObject = new StringContent(filteredResponse, Encoding.UTF8, "application/json");
        
        response.Content = newContentObject;

        return response;
    }

    private static string ReplaceResponseFields(string originalBodyResponse, Provider provider)
    {
        return provider.ResponseFormat.Aggregate(originalBodyResponse,
            (current, field) => current.Replace(field.SourceName, field.TargetName));
    }

    private static string GetResponseWithSelectedFields(string originalResponseBody, Resource resource, string providerName)
    {
        var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(originalResponseBody);
        var filteredResponse = resource.ResponseFields
            .ToDictionary(field => field, field => jsonResponse![field]);
        
        filteredResponse.Add("provider", providerName);

        return JsonConvert.SerializeObject(filteredResponse);
    }
}