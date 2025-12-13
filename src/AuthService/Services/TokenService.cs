using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Configuration;
using AuthService.Data;
using Shared.Models;
namespace AuthService.Services;
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IOptions<JwtSettings> jwtSettings, UserManager<ApplicationUser> userManager, ILogger<TokenService> logger
    )
    {
        _jwtSettings = jwtSettings.Value;
        _userManager = userManager;
        _logger = logger;
    }

    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        // JWT contains ONLY authentication data - no profile data
        // Profile data (firstName, lastName, etc.) is managed by UserService
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,

            audience: _jwtSettings.Audience,

            claims: claims,

            expires: expires,

            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != refreshToken)
        {
            return false;
        }

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return false;
        }
        return true;
    }

    public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if(user != null)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<string> GenerateTokenWithSessionAsync(ApplicationUser user, string sessionId)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti,sessionId),
            new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role,role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("JWT token generated for user {userID} with SessionId {sessionId}", user.Id, sessionId);

        return tokenString;
    }
}
