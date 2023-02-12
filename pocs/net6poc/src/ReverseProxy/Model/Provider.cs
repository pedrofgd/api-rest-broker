namespace ReverseProxy.Model;

public class Provider
{
    public string Name { get; set; }
    public string Path { get; set; }
    public List<ResponseFormat> ResponseFormat { get; set; }
}