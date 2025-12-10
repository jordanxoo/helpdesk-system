namespace Shared.Caching;


public static class CacheKeys
{
    
    //Users
    public static string User(Guid id) => $"user:{id}";
    public static string UserByEmail(string email) => $"user:email:{email.ToLowerInvariant()}";

    //Tickets
    public static string Ticket(Guid id) => $"ticket:{id}";
    public static string TicketStatistics() =>"ticket:statistics";
    public static string UserTicketsCount(Guid userID) => $"user:{User}:tickets:count";

    //Sesions

    public static string TokenBlacklist(string jti) => $"blacklist:token:{jti}";
    public static string UserSession(Guid userId, string sessionId) => $"session:{userId}:{sessionId}";

    //rate limiting 
    public static string RateLimit(string clientId, DateTime window) => $"ratelimit:{clientId}:w{window:yyyyMMddHHmm}";

    //notifications

    public static string PendingNotifications(Guid userId) => $"notifications:{userId}:pending";
    public static string UnreadCount(Guid userId) => $"notifications:{userId}:unread_count";

}