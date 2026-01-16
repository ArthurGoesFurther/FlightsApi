using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    // keep registry of keys to support pattern removal since IMemoryCache doesn't provide it
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte> _keys =
        new System.Collections.Concurrent.ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out var value) && value is string json)
        {
            var result = JsonSerializer.Deserialize<T>(json);
            return Task.FromResult(result);
        }
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        var options = new MemoryCacheEntryOptions();
        
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }
        else
        {
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
        }

        _cache.Set(key, json, options);
        _keys.TryAdd(key, 0);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return Task.CompletedTask;
        }

        // Support simple wildcard pattern ending with '*', e.g. 'flights_*'
        if (pattern.EndsWith("*", StringComparison.Ordinal))
        {
            var prefix = pattern.Substring(0, pattern.Length - 1);
            var keysToRemove = _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _keys.TryRemove(key, out _);
            }
            return Task.CompletedTask;
        }

        // Exact match
        _cache.Remove(pattern);
        _keys.TryRemove(pattern, out _);
        return Task.CompletedTask;
    }
}
