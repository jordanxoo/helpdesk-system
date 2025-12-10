using Microsoft.Extensions.Caching.Distributed;
using Shared.Caching;
using Shared.Models;
using System.Text.Json;



namespace NotificationService.Services;

/// <summary>
/// Implementacja kolejki powiadomień używająca Redis.
/// 
/// Struktura danych redis:
/// - notifications:{userId}:pending - lista powiadomień 
/// - notifications:{userId}:unread_count - licznik 
/// 
/// Każda lista ma TTL 7 dni 
/// </summary>

public class RedisNotificationQueueService : INotificationQueueService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisNotificationQueueService> _logger;

    private const int MaxNotificationsPerUser = 100;
    private static readonly TimeSpan NotificationTTL = TimeSpan.FromDays(7);

    public RedisNotificationQueueService(IDistributedCache cache, ILogger<RedisNotificationQueueService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task EnqueueNotificationAsync(
        Guid userId,
        NotificationMessage message,
        CancellationToken ct = default
    )
    {
        var cachedKey = CacheKeys.PendingNotifications(userId);

        var existingJson = await _cache.GetStringAsync(cachedKey,ct);
        var notifications = string.IsNullOrEmpty(existingJson)
        ? new List<NotificationMessage>()
        : JsonSerializer.Deserialize<List<NotificationMessage>>(existingJson);

        if(notifications is not null)
        {
        notifications.Add(message);

        if(notifications.Count > MaxNotificationsPerUser)
        {
            notifications = notifications.TakeLast(MaxNotificationsPerUser).ToList();
            _logger.LogWarning("User {UserID} exceeded max notifications limit. Oldest notification removed", userId);
        }
        await _cache.SetStringAsync(
            cachedKey,
            JsonSerializer.Serialize(notifications),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = NotificationTTL
            },
            ct);

            await IncrementUnreadCountAsync(userId,ct);

            _logger.LogInformation(
                "Notification enqueued for user {userId}: {type} - {title}",
                userId,message.type,message.title
            );
        }
    }
     public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        var countStr = await _cache.GetStringAsync(CacheKeys.UnreadCount(userId), ct);
        return string.IsNullOrEmpty(countStr) ? 0 : int.Parse(countStr);
    }
    public async Task<IEnumerable<NotificationMessage>> GetPendingNotificationsAsync(
        Guid userId,
        CancellationToken ct = default
    )
    {
        var cacheKey = CacheKeys.PendingNotifications(userId);
        var json = await _cache.GetStringAsync(cacheKey,ct);

        if(string.IsNullOrEmpty(json))
        {
            _logger.LogDebug("No pending notifications for user {UserId}",userId);
            return Enumerable.Empty<NotificationMessage>();
        }

        var notifications = JsonSerializer.Deserialize<List<NotificationMessage>>(json)!;

        _logger.LogDebug("Retrieved {count} pending notifications for user {userId}",notifications.Count,userId);
        return notifications;
    }

    public async Task MarkAsDeliveredAsync(
        Guid userId,
        string notificationID,
        CancellationToken ct = default
    )
    {
        var cacheKey = CacheKeys.PendingNotifications(userId);
        var json = await _cache.GetStringAsync(cacheKey,ct);

        if(string.IsNullOrEmpty(json))
        {
            _logger.LogWarning("Attempted to mark notification {notificationId} as delivered for user {UserID}, but no notification found",
                notificationID, userId);
                return;
        }

        var notifications = JsonSerializer.Deserialize<List<NotificationMessage>>(json)!;
        var nitialcount = notifications.Count;

        var removedCount = notifications.RemoveAll(n => n.id == notificationID);

        if(removedCount == 0)
        {
            _logger.LogWarning("Notification {notificationId} not found for user {userId}",
            notificationID, userId);
            return;
        }

        if(notifications.Count > 0)
        {
            await _cache.SetStringAsync(cacheKey,
            JsonSerializer.Serialize(notifications),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = NotificationTTL
            },ct);
        }
        else
        {
            await _cache.RemoveAsync(cacheKey,ct);
        }
        await DecrementUnreadCountAsync(userId,ct);
        _logger.LogInformation(
            "Notification {notificationID} marked as delivered for user {userID}",
            notificationID,userId);
    }

    public async Task ClearAllNotificationsAsync(Guid userID, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(CacheKeys.PendingNotifications(userID),ct);
        await _cache.RemoveAsync(CacheKeys.UnreadCount(userID),ct);

        _logger.LogInformation("All notifications cleared for user {userID}",userID);
    }


    private async Task IncrementUnreadCountAsync(Guid userID, CancellationToken ct = default)
    {
        var current = await GetUnreadCountAsync(userID,ct);
        await _cache.SetStringAsync(
            CacheKeys.UnreadCount(userID),
            (current + 1).ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = NotificationTTL
            },
        ct);
    }
    private async Task DecrementUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        var current = await GetUnreadCountAsync(userId,ct);
        var newCount = Math.Max(0,current - 1);

        if(newCount == 0)
        {
            await _cache.RemoveAsync(CacheKeys.UnreadCount(userId));
        }
        else
        {
            await _cache.SetStringAsync(
                CacheKeys.UnreadCount(userId),
                newCount.ToString(),
                ct);
        }
    }

}