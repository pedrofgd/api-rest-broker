using System.Text.RegularExpressions;
using ReverseProxy.Model;

namespace ReverseProxy.Resolver;

public class TargetResolver
{
    public Resource? Resource { get; set; }
    public Provider? TargetProvider { get; set; }
    public Uri? TargetUri { get; set; }
    
    private readonly List<Resource> _settings;

    public TargetResolver(IConfiguration configuration)
    {
        _settings = configuration.GetSection("Resources")
            .Get<List<Resource>>()!;
    }
    
    public void BuildTargetUri(HttpRequest request)
    {
        var (resource, routeParameters) = DeconstructPath(request.Path);
        SetResource(resource);
        SetAvailableProvider();
        TargetUri = new Uri(ReplaceRouteParameters(routeParameters, TargetProvider.Path));
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

    private void SetResource(string resourceName)
    {
        Resource = _settings.FirstOrDefault(settings => settings.Name == resourceName)!;
    }

    private void SetAvailableProvider()
    {
        var providers = Resource.Providers;
        var random = new Random();
        TargetProvider = providers[random.Next(providers.Count)];
    }

    private string ReplaceRouteParameters(IEnumerable<string> parameters, string providerPath)
    {
        var regex = new Regex("{(.*?)}");
        providerPath = parameters.Aggregate(providerPath, (current, parameter) => 
            regex.Replace(current, parameter));

        return providerPath;
    }
}