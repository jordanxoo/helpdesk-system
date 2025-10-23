namespace AuthService.Services;

using AuthService.Data;
using Shared.DTOs;
public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    Task<bool> ValidateRefreshTokenAsync(string UserId, string refreshToken);
    public  Task SaveRefreshTokenAsync(string userId, string refreshToken);
    
}
