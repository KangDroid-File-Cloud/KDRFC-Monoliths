using Newtonsoft.Json;
using Shared.Core.Abstractions;
using StackExchange.Redis;

namespace Shared.Infrastructure.Persistence;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    // Since IDatabase is Cheap Thru-pass object, always define as "Getter"
    private IDatabase CacheDatabase => _connectionMultiplexer.GetDatabase();

    public CacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task SetItemAsync(string key, object data, TimeSpan? expiry = null)
    {
        await CacheDatabase.StringSetAsync(key, JsonConvert.SerializeObject(data), expiry ?? TimeSpan.FromMinutes(10));
    }

    public async Task<TItem> GetItemOrCreateAsync<TItem>(string key, Func<Task<TItem>> valueFactory, TimeSpan? expiry = null)
    {
        TItem item;

        // 1. Try to get data from cache
        var value = await CacheDatabase.StringGetAsync(key);

        // 2. If cache does not have value
        if (!value.HasValue)
        {
            // 2 - 1. Set Cache Value
            item = await valueFactory();
            if (item != null)
                await CacheDatabase.StringSetAsync(key, JsonConvert.SerializeObject(item), expiry ?? TimeSpan.FromMinutes(10));
        }
        else
        {
            // 3. Cache has value, so just deserialize it.
            item = JsonConvert.DeserializeObject<TItem>(value);
        }

        return item;
    }

    public async Task DeleteItemAsync(string key)
    {
        await CacheDatabase.KeyDeleteAsync(key);
    }
}