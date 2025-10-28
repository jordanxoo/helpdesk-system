namespace Shared.Constants;

/// <summary>
/// Centralized user role constants for authorization.
/// Used in [Authorize(Roles = ...)] attributes and role seeding.
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// Customer role - can create and view their own tickets.
    /// </summary>
    public const string Customer = "Customer";

    /// <summary>
    /// Agent role - can be assigned tickets and handle customer requests.
    /// </summary>
    public const string Agent = "Agent";

    /// <summary>
    /// Administrator role - full system access and user management.
    /// </summary>
    public const string Administrator = "Administrator";
}
