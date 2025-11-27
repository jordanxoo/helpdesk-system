namespace Shared.Models;

/// <summary>
/// User - UserService Domain Model
///
/// UserService is the OWNER of all user profile and business data.
/// AuthService only stores authentication credentials (email, password).
///
/// Data ownership:
/// - Profile: Email, FirstName, LastName, PhoneNumber, Role (owned by UserService)
/// - Business: OrganizationId, IsActive (owned by UserService)
/// - Auth: Password (owned by AuthService - not in this model)
///
/// Sync: UserRegisteredEvent from AuthService creates initial profile.
/// </summary>
public class User
{
    public Guid Id { get; set; }                // Shared with AuthService.ApplicationUser.Id
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? OrganizationId { get; set; }   // FK to Organization (optional)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}";
}

public enum UserRole
{
    Customer,      // Klient zgłaszający problemy
    Agent,         // Agent pomocy technicznej
    Administrator  // Administrator systemu
}
