namespace Shared.Core.Abstractions;

public interface ICacheService
{
    public Task SetItemAsync(string key, object data, TimeSpan? expiry = null);
    public Task<TItem> GetItemOrCreateAsync<TItem>(string key, Func<Task<TItem>> valueFactory, TimeSpan? expiry = null);
    public Task DeleteItemAsync(string key);
}