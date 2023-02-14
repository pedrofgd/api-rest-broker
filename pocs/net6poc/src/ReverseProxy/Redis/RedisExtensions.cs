using Newtonsoft.Json;
using ReverseProxy.Model;
using Serilog;
using StackExchange.Redis;

namespace ReverseProxy.Redis;

public static class RedisExtensions
{
    public static string ProvidersKey = "providers";

    public static IServiceCollection AddRedis(this IServiceCollection services,
        IConfiguration configuration)
    {
        var multiplexer = ConnectionMultiplexer.Connect("127.0.0.1:6379");

        var db = multiplexer.GetDatabase();

        var resources = configuration
            .GetSection("Resources")
            .Get<List<Resource>>();

        foreach (var resource in resources)
        {
            var providerCount = 1;
            foreach (var provedor in resource.Providers)
            {
                var member = JsonConvert.SerializeObject(provedor);
                var result = db.SortedSetAdd(resource.Name, member, providerCount);
                Log.Logger.Information($"{provedor.Name} added to Redis? {result}");
                providerCount++;
            }
        }

        services.AddSingleton<IConnectionMultiplexer>(multiplexer);

        return services;
    }

    public static void CleanUp()
    {
        Log.Logger.Information("Cleaning Up Redis");
        var conn = ConnectionMultiplexer.Connect("127.0.0.1:6379,allowAdmin=true");
        
        // get the target server
        var server = conn.GetServer("127.0.0.1:6379");

        // show all keys in database 0 that include "foo" in their name
        foreach(var key in server.Keys(pattern: "*")) {
            Console.WriteLine(key);
        }

        // completely wipe ALL keys from database 0
        server.FlushDatabase();
    }
}