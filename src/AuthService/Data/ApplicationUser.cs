using Microsoft.AspNetCore.Identity;

namespace AuthService.Data;

/// <summary>
/// ApplicationUser - Authentication Context ONLY
/// Stores credentials and authentication-related data.
/// Profile data (FirstName, LastName, etc.) is managed by UserService.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // Email, PasswordHash inherited from IdentityUser - used for login

    public DateTime CreatedAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
