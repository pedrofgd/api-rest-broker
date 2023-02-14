using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ReverseProxy.Model;
using Serilog;
using StackExchange.Redis;

namespace ReverseProxy.Resolver;

public class TargetResolver
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDistributedCache _cache;
    private readonly List<Resource> _settings;

    public TargetResolver(
        IConfiguration configuration,
        IConnectionMultiplexer redis,
        IDistributedCache cache)
    {
        _redis = redis;
        _cache = cache;
        _settings = configuration.GetSection("Resources")
            .Get<List<Resource>>()!;
    }
    
    public ProviderResolved ResolveTarget(HttpRequest request)
    {
        var (resourceName, routeParameters) = DeconstructPath(request.Path);
        var resource = GetResource(resourceName);
        var provider = GetAvailableProvider(resource);
        var uri = new Uri(ReplaceRouteParameters(routeParameters, provider.Path));
        return new ProviderResolved(resource, provider, uri);
    }

    private (string, string[]) DeconstructPath(PathString requestedPath)
    {
        if (!requestedPath.StartsWithSegments("/api", out var remainingPath)) 
            return (null, null)!;
        
        var deconstructed = remainingPath.Value!.Split("/")[1..];
        var resource = deconstructed[0];
        var routeParameters = deconstructed[1..];

        return (resource, routeParameters);
    }

    private Resource GetResource(string resourceName)
    {
        return _settings.FirstOrDefault(settings => settings.Name == resourceName)!;
    }

    private Provider GetAvailableProvider(Resource resource)
    {
        // Connection Multiplexer
        var watchGetFromRedis = Stopwatch.StartNew();
        var db = _redis.GetDatabase();
        var providers = db.SortedSetRangeByRankWithScores(resource.Name);
        watchGetFromRedis.Stop();
        Log.Logger.Information("Providers loaded from Redis in {Elapsed}ms", watchGetFromRedis.ElapsedMilliseconds);
        
        Log.Logger.Information("Available providers for resource {ResourceName}: {Providers}",
            resource.Name, providers.Select(x => x.Element));

        var mostAvailableProvider = JsonConvert.DeserializeObject<Provider>(providers.First().Element!)!;
        Log.Logger.Information("Moving with {ProviderName}", mostAvailableProvider);

        // Distributed cache
        var id = Guid.NewGuid();
        _cache.Set(resource.Name + id, Encoding.UTF8.GetBytes(providers.First().Element));
        var watchGetCached = Stopwatch.StartNew();
        var cached = _cache.Get(resource.Name + id);
        watchGetCached.Stop();
        Log.Logger.Information("Spent {Milliseconds}ms to get cached content", watchGetCached.ElapsedMilliseconds);
        var cachedContent = Encoding.UTF8.GetString(cached);
        Log.Logger.Information("Cached content = {Cached}", cachedContent);

        return mostAvailableProvider;
    }

    private string ReplaceRouteParameters(IEnumerable<string> parameters, string providerPath)
    {
        var regex = new Regex("{(.*?)}");
        providerPath = parameters.Aggregate(providerPath, (current, parameter) => 
            regex.Replace(current, parameter));

        return providerPath;
    }
}