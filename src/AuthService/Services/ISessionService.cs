using Shared.Models;


namespace AuthService.Services;




public interface ISessionService
{
    /// <summary>
    /// tworzymy nowa sesje po zalogowaniu
    /// zapisujemy sesje w redis i zwracamy session id => do umieszczenia w JWT jako jwt claim
    /// </summary>
    Task<string> CreateSessionAsync(
        Guid userId,
        string deviceInfo,
        string ipAddress,
        DateTime expiresAt,
        CancellationToken ct = default
    );


    /// <summary>
    /// wylogowuje uzytkownika dodaje token do blacklsit, token jest niewazny mimo ze nie wygasl
    /// </summary>
    
    Task RevokeSessionAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// wylogowuje uzytkownika ze wzsystkich urzadzen
    /// </summary>
    
    Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct = default);


    /// <summary>
    /// sprawdza czy token jest na blackliscie
    /// wywolywane w middleware przed przetworzenien requestu
    /// </summary>
    
    Task<bool> IsSessionRevokedAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// pobiera wszystkie aktywne sejse uzytkownika
    /// uzywane do wyswietlenia listy zalogowanych urzadzen w UI
    /// </summary>
    Task<IEnumerable<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken ct = default);


    /// <summary> 
    /// usuwa wygasle sesje z redis
    /// wywolywane cykliczne np co godzine xd
    /// </summary>
    Task CleanupExpiredSessionsAsync(CancellationToken ct = default);
}