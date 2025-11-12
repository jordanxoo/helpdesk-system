namespace Shared.DTOs;

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Role  // Customer, Agent, or Administrator
);

public record RefreshTokenRequest(
    string RefreshToken
);

/// <summary>
/// Wymogi haseł dla walidacji po stronie frontendu
/// Wartości są ustawiane dynamicznie z konfiguracji Identity w AuthService/Program.cs
/// </summary>
public record PasswordRequirements
{
    public int MinimumLength { get; init; }
    public bool RequireDigit { get; init; }
    public bool RequireLowercase { get; init; }
    public bool RequireUppercase { get; init; }
    public bool RequireNonAlphanumeric { get; init; }
    
    /// <summary>
    /// Czytelny opis wymogów dla użytkownika
    /// </summary>
    public string Description => 
        $"Hasło musi mieć minimum {MinimumLength} znaków" +
        (RequireLowercase ? ", małą literę" : "") +
        (RequireUppercase ? ", dużą literę" : "") +
        (RequireDigit ? ", cyfrę" : "") +
        (RequireNonAlphanumeric ? ", znak specjalny" : "") +
        ".";
}

