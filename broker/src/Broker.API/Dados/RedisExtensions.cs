using StackExchange.Redis;

namespace Broker.API.Dados;

public static class RedisExtensions
{
    public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisHost = configuration["RedisCache:Host"];
        ArgumentException.ThrowIfNullOrEmpty(redisHost);

        var multiplexer = ConnectionMultiplexer.Connect(redisHost);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
    }
}