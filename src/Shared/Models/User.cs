namespace Shared.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? OrganizationId { get; set; }  // FK to Organization (optional)
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
