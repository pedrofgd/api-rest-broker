namespace ReverseProxy.Model;

public class Resource
{
    public string Name { get; set; }
    public List<Provider> Providers { get; set; }
    public string[] ResponseFields { get; set; }
}