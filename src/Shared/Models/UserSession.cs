namespace Shared.Models;






public record UserSession(
    string SessionId,
    Guid UserId,
    string DeviceInfo,
    string IpAddress,
    DateTime LoginTime,
    DateTime ExpiresAt,
    bool IsActive = true
)
{

    //tworzenie nowej sesji z automatycznym czasem logowania
    public static UserSession Create(
        string sessionId,
        Guid userId,
        string deviceInfo,
        string ipAddress,
        DateTime expiresAt
    )
    {
        return new UserSession(
            SessionId: sessionId,
            UserId: userId,
            DeviceInfo: deviceInfo,
            IpAddress: ipAddress,
            LoginTime: DateTime.UtcNow,
            ExpiresAt: expiresAt
        );
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    
    public UserSession Deactivate() => this with {IsActive = false};


}