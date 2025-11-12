namespace Shared.Events;

/// <summary>
/// Event published when a new user registers in the system.
/// UserService should create a corresponding user record.
/// </summary>
public record UserRegisteredEvent : BaseEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}

/// <summary>
/// Event published when user profile is updated in AuthService.
/// UserService should sync the changes (eventual consistency).
/// </summary>
public record ProfileUpdatedEvent : BaseEvent
{
    public Guid UserId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Role { get; init; }
}
