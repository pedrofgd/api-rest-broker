using ReverseProxy.Model;

namespace ReverseProxy.Resolver;

public class ProviderResolved
{
    public Resource Resource { get; set; }
    public Provider TargetProvider { get; set; }
    public Uri TargetUri { get; set; }

    public ProviderResolved(Resource resource, Provider provider, Uri uri)
    {
        Resource = resource;
        TargetProvider = provider;
        TargetUri = uri;
    }
}