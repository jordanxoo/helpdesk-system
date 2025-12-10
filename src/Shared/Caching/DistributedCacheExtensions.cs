using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Shared.Caching;


public static class DistributedCacheExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

/// <summary>
/// pobierze obiekt z achce i deserializuje go z JSON
/// </summary>
/// <typeparam name="T">Typ obiektu do pobrania, domniemany</typeparam> 
/// <param name="cache">instancja Redis</param>
/// <param name="key">klucz w redis</param>
/// <param name="ct">Token anulowania</param>
/// <returns></returns>
    public static async Task<T?> GetAsync <T>(
        this IDistributedCache cache,
        string key,
        CancellationToken ct = default
    ) where T : class
    {
        var json = await cache.GetStringAsync(key,ct);

        if(string.IsNullOrEmpty(json))
        {
            return null;
        }
        return JsonSerializer.Deserialize<T>(json,JsonOptions);
    }
/// <summary>
/// zapisuje obiekt do cache jako json
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="cache"></param>
/// <param name="key"></param>
/// <param name="value">obiekt do zapisania</param>
/// <param name="absoluteExp">kiedy wygasnie klucz</param>
/// <param name="slidingExp">reset przy kazdym dostepie</param>
/// <param name="ct"></param>
/// <returns></returns>
    public static async Task SetAsync<T>(
        this IDistributedCache cache,
        string key,
        T value,
        TimeSpan? absoluteExp = null,
        TimeSpan? slidingExp = null,
        CancellationToken ct = default
    ) where T : class
    {
        var options = new DistributedCacheEntryOptions();

        if(absoluteExp.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = absoluteExp;
        }

        if(slidingExp.HasValue)
        {
            options.SlidingExpiration = slidingExp;
        }

        var json = JsonSerializer.Serialize(value,JsonOptions);

        await cache.SetStringAsync(key,json,options,ct);
    }

    public static async Task<T> GetOrSetAsync<T>(
        this IDistributedCache cache,
        string key,
        Func<Task<T>> factory,
        TimeSpan? absoluteExp = null,
        CancellationToken ct = default
    ) where T : class
    {
        var cached = await cache.GetAsync<T>(key,ct);

        if(cached is not null)
        {
            return cached;
        }

        var value = await factory();

        await cache.SetAsync(key,value,absoluteExp,ct: ct);
        return value;
    }

}



