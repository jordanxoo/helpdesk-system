namespace Shared.Exceptions;

/// <summary>
/// Exception thrown when a user is not found (e.g., deleted account)
/// </summary>
public class UserNotFoundException : Exception
{
    public Guid UserId { get; }

    public UserNotFoundException(Guid userId)
        : base($"User with id '{userId}' was not found (account may have been deleted).")
    {
        UserId = userId;
    }
}
