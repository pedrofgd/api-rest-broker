using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ReverseProxy.Redis;

public class RedisCache : IDistributedCache
{
    private readonly IDatabase _database;

    public RedisCache(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public byte[] Get(string key)
    {
        return _database.StringGet(key)!;
    }

    public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
    {
        return (await _database.StringGetAsync(key))!;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        _database.StringSet(key, value, options.AbsoluteExpiration - DateTime.Now);
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    { 
        await _database.StringSetAsync(key, value);
    }

    public void Refresh(string key)
    {
        _database.KeyExpire(key, TimeSpan.FromSeconds(30));
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        await _database.KeyExpireAsync(key, TimeSpan.FromSeconds(30));
    }

    public void Remove(string key)
    {
        _database.KeyDelete(key);
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        return _database.KeyDeleteAsync(key);
    }
}
