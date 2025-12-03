namespace Shared.Events;

/// <summary>
/// Event published when a new user registers in the system.
/// Contains FULL profile data that UserService will store.
/// AuthService only stores credentials (email, password) - profile data is passed via this event.
/// </summary>
public record UserRegisteredEvent : BaseEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}

public record UserLoggedInEvent : BaseEvent
{
    public Guid UserId {get;init;}

    public string Email {get;init;}
}
