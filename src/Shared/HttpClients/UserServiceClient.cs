using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Shared.DTOs;
using Shared.Exceptions;

namespace Shared.HttpClients;

/// <summary>
/// HTTP client implementation for UserService communication.
/// Uses IHttpClientFactory for proper connection pooling and resilience.
/// </summary>
public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("Fetching user details from UserService: {UserId}", userId);
            
            var response = await _httpClient.GetAsync($"/api/users/{userId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("User {UserId} not found (deleted account)", userId);
                throw new UserNotFoundException(userId);
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("UserService returned Unauthorized for user {UserId}", userId);
                return null;
            }
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("UserService returned {StatusCode} for user {UserId}", 
                    response.StatusCode, userId);
                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            
            return user;
        }
        catch (UserNotFoundException)
        {
            throw; // Re-throw to distinguish from other errors
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching user {UserId} from UserService", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching user {UserId} from UserService", userId);
            return null;
        }
    }

    public async Task<Guid?> GetUserOrganizationAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("Fetching organization for user {UserId} from UserService", userId);
            
            // Reuse GetUserAsync and extract OrganizationId
            // In production, you might want a dedicated lightweight endpoint
            var user = await GetUserAsync(userId);
            
            if (user == null)
            {
                _logger.LogDebug("User {UserId} not found in UserService", userId);
                return null;
            }
            
            return user.OrganizationId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching organization for user {UserId}", userId);
            return null;
        }
    }
}
