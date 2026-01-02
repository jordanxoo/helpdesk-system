using Microsoft.Extensions.Caching.Distributed;
using Shared.Models;
using System.Text.Json;



namespace AuthService.Services;

public class RedisSessionService : ISessionService
{
    private readonly IDistributedCache _cache;
    private  readonly ILogger<RedisSessionService> _logger;


    public RedisSessionService(IDistributedCache cache, ILogger<RedisSessionService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> CreateSessionAsync(Guid userId, 
    string deviceInfo, 
    string ipAddress, 
    DateTime expiresAt,
    CancellationToken ct = default)
    {
        var sessionId = Guid.NewGuid().ToString();

        var session = UserSession.Create(
            sessionId: sessionId,
            userId: userId,
            deviceInfo: deviceInfo,
            ipAddress: ipAddress,
            expiresAt: expiresAt
        );

        var cacheKey = GetSessionKey(userId,sessionId);

        var ttl = expiresAt - DateTime.UtcNow;

        await _cache.SetStringAsync(cacheKey,JsonSerializer.Serialize(session),new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        },ct);

        await AddSessionToUserListAsync(userId,sessionId,ct);
        await AddUserIdAsync(userId,ct);

        _logger.LogInformation(
            "Session created for user {userId} from {ipAddress} {DeviceInfo}, SessionId {sessionId}",
            userId,ipAddress,deviceInfo,sessionId
        );
        return sessionId;
    }

    public async Task RevokeSessionAsync(string sessionId,CancellationToken ct = default)
    {
        var blacklistKey = GetBlacklistKey(sessionId);

        await _cache.SetStringAsync(
            blacklistKey,
            "revoked",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            },ct);

            var allUserIds = await GetAllUserIdsAsync(ct);
            foreach(var uid in allUserIds)
            {
                var sessions = await GetActiveSessionsAsync(uid,ct);
                if(sessions.Any(s => s.SessionId == sessionId))
                {
                    await RemoveSessionsFromUserListAsync(uid,sessionId,ct);
                    break;
                }
            }

            _logger.LogInformation("Session {sessionId} revoked and added to blacklist", sessionId);
    }

    public async Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        var sessions = await GetActiveSessionsAsync(userId,ct);

        foreach(var session in sessions)
        {
            await RevokeSessionAsync(session.SessionId,ct);
        }
        _logger.LogWarning(
            "All session revoked for user {userId}. Total sessions: {count}"
            ,userId,sessions.Count()
        );
    }

    public async Task<bool> IsSessionRevokedAsync(string sessionId,CancellationToken ct)
    {
        var blackListKey = GetBlacklistKey(sessionId);

        var value = await _cache.GetStringAsync(blackListKey,ct);

        var isRevoked = !string.IsNullOrEmpty(value);
        
        if(isRevoked)
        {
            _logger.LogWarning(
                "Blocked request with revoked session: {sessionId}", sessionId
            );
        }
        return isRevoked;
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken ct)
    {
        var listKey = GetUserSessionListKey(userId);
        var sessionIdsJson = await _cache.GetStringAsync(listKey,ct);

        if(string.IsNullOrEmpty(sessionIdsJson))
        {
            return Enumerable.Empty<UserSession>();
        }

        var sessionIds = JsonSerializer.Deserialize<List<string>>(sessionIdsJson) ?? new List<string>();

        var sessions = new List<UserSession>();

        foreach(var sessionId in sessionIds)
        {
            var sessionKey = GetSessionKey(userId,sessionId);
            var sessionJson = await _cache.GetStringAsync(sessionKey,ct);

            if(!string.IsNullOrEmpty(sessionJson))
            {
                var session = JsonSerializer.Deserialize<UserSession>(sessionJson);

                if(session != null && session.IsActive && !session.IsExpired())
                {
                    sessions.Add(session);
                }
            }
        }
        return sessions;
    }

    public async Task CleanupExpiredSessionsAsync(CancellationToken ct = default)
    {
        var cleanedSessions = 0;
        var cleanedUsers = 0;

        try
        {
            var userIds = await GetAllUserIdsAsync(ct);
            foreach(var userId in userIds)
            {
                var listKey = GetUserSessionListKey(userId);
                var sessionIdsJson = await _cache.GetStringAsync(listKey,ct);

                if(string.IsNullOrEmpty(sessionIdsJson))
                {
                    continue;
                }

                var sessionIds = JsonSerializer.Deserialize<List<string>>(sessionIdsJson) ?? new List<string>();

                var validSessionIds = new List<string>();

                foreach(var sessionId in sessionIds)
                {
                    var sessionKey = GetSessionKey(userId,sessionId);
                    var sessionJson = await _cache.GetStringAsync(sessionKey,ct);

                    if(string.IsNullOrEmpty(sessionJson))
                    {
                        cleanedSessions++;
                        _logger.LogDebug(
                            "Removed expired session {sessionId} from user {userId} list",sessionId, userId
                        );
                        continue;
                    }

                    var session = JsonSerializer.Deserialize<UserSession>(sessionJson);

                    if(session != null  && !session.IsExpired())
                    {
                        validSessionIds.Add(sessionId);
                    }
                    else
                    {
                        await _cache.RemoveAsync(sessionKey,ct);
                        cleanedSessions++;
                        _logger.LogDebug(
                            "Removed expired session {sessionId} for user {userId}",sessionId,userId);
                    }

                }
                if(validSessionIds.Count > 0)
                {
                    await _cache.SetStringAsync(
                        listKey,
                        JsonSerializer.Serialize(validSessionIds),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
                        },ct);
                }
                else
                {
                    await _cache.RemoveAsync(listKey,ct);

                    await RemoveUserIdAsync(userId,ct);
                    cleanedUsers++;

                    _logger.LogInformation("Removed all sessions for user {userId}, user removed from active list",userId);
                }
            }
            _logger.LogInformation("Session cleanup completed. Cleaned {sessionCount} sessions from {userCount} users", cleanedSessions, cleanedUsers);
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error during session cleanup");
        }   
    }

    private async Task AddSessionToUserListAsync(Guid userId, string sessionId, CancellationToken ct)
    {
        var listKey = GetUserSessionListKey(userId);
        var sessionIdsJson = await _cache.GetStringAsync(listKey,ct);

        var sessionIds = string.IsNullOrEmpty(sessionIdsJson) ?
        new List<string>() : JsonSerializer.Deserialize<List<string>>(sessionIdsJson) ?? new List<string>();

        if(!sessionIds.Contains(sessionId))
        {
            sessionIds.Add(sessionId);

            await _cache.SetStringAsync(listKey,
            JsonSerializer.Serialize(sessionIds),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            },ct);
        }
    }

    public async Task RemoveSessionsFromUserListAsync(Guid userId,string sessionId,CancellationToken ct)
    {
        var listkey = GetUserSessionListKey(userId);

        var sessionIdsJson = await _cache.GetStringAsync(listkey,ct);

        if(string.IsNullOrEmpty(sessionIdsJson))
        {
            return;
        }

        var sessionIds = JsonSerializer.Deserialize<List<string>>(sessionIdsJson) ?? new List<string>();

        sessionIds.Remove(sessionId);

        if(sessionIds.Count > 0)
        {
            await _cache.SetStringAsync(listkey,JsonSerializer.Serialize(sessionIds),new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            },ct);
        }
        else
        {
            await _cache.RemoveAsync(listkey,ct);
        }
    }

    private async Task<List<Guid>> GetAllUserIdsAsync(CancellationToken ct)
    {
        const string userListKey = "sessions:users";
        var userIdsJson = await _cache.GetStringAsync(userListKey,ct);

        if(string.IsNullOrEmpty(userIdsJson))
        {
            return new List<Guid>();
        }

        return JsonSerializer.Deserialize<List<Guid>>(userIdsJson) ?? new List<Guid>();
    }

    private async Task AddUserIdAsync(Guid userId, CancellationToken ct)
    {
        const string userListKey = "sessions:users";

        var userIds = await GetAllUserIdsAsync(ct);

        if(!userIds.Contains(userId))
        {
            userIds.Add(userId);

            await _cache.SetStringAsync(userListKey,JsonSerializer.Serialize(userIds),new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            },ct);
        }
    }
    private async Task RemoveUserIdAsync(Guid userId, CancellationToken ct)
    {
        const string userListKey = "sessions:users";
        var userIds = await GetAllUserIdsAsync(ct);

        if(userIds.Remove(userId))
        {
            if(userIds.Count > 0)
            {
                await _cache.SetStringAsync(userListKey,JsonSerializer.Serialize(userIds),new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
                },ct);
            }
            else
            {
                await _cache.RemoveAsync(userListKey,ct);
            }
        }
    }

    public static string GetSessionKey(Guid userId, string sessionId) => $"sessions:{userId}:{sessionId}";

    public static string GetBlacklistKey(string sessionId) => $"blacklist:{sessionId}";

    public static string GetUserSessionListKey(Guid userId) => $"sessions:{userId}:list";

    public async Task<int> CleanupExpiredSessionsAsync()
    {
        _logger.LogInformation("Starting expired sessions cleanup job");

        var cleanedCount = 0;
        
        // Pobierz wszystkich userów którzy mają sesje
        var allUserIds = await GetAllUserIdsAsync(CancellationToken.None);

        if(allUserIds == null || allUserIds.Count == 0)
        {
            _logger.LogInformation("No user sessions found to clean");
            return 0;
        }

        foreach(var userId in allUserIds)
        {
            var userSessionListKey = GetUserSessionListKey(userId);
            var sessionsJson = await _cache.GetStringAsync(userSessionListKey);

            if(string.IsNullOrEmpty(sessionsJson))
            {
                continue;
            }
            
            var sessionIds = JsonSerializer.Deserialize<List<string>>(sessionsJson) ?? new List<string>();
            var validSessionIds = new List<string>();

            foreach(var sessionId in sessionIds)
            {
                var sessionKey = GetSessionKey(userId, sessionId);
                var sessionJson = await _cache.GetStringAsync(sessionKey);

                if(string.IsNullOrEmpty(sessionJson))
                {
                    // Session wygasła
                    cleanedCount++;
                    _logger.LogDebug("Removed expired session: {SessionId} for user {UserId}", sessionId, userId);
                }
                else
                {
                    validSessionIds.Add(sessionId);
                }
            }

            // Zaktualizuj listę sesji jeśli coś się zmieniło
            if(validSessionIds.Count != sessionIds.Count)
            {
                if(validSessionIds.Count > 0)
                {
                    await _cache.SetStringAsync(
                        userSessionListKey,
                        JsonSerializer.Serialize(validSessionIds),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
                        }
                    );
                }
                else
                {
                    // Brak sesji - usuń listę i użytkownika
                    await _cache.RemoveAsync(userSessionListKey);
                    await RemoveUserIdAsync(userId, CancellationToken.None);
                }   
            }
        }
        
        _logger.LogInformation("Cleaned up {Count} expired sessions", cleanedCount);
        return cleanedCount;
    }
}