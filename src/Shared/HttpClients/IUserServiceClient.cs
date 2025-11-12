using Shared.DTOs;

namespace Shared.HttpClients;

/// <summary>
/// HTTP client for communication with UserService.
/// Used by TicketService and NotificationService to fetch user details.
/// </summary>
public interface IUserServiceClient
{
    /// <summary>
    /// Get full user details by ID.
    /// </summary>
    Task<UserDto?> GetUserAsync(Guid userId);
    
    /// <summary>
    /// Get only organization ID for a user (optimized query).
    /// </summary>
    Task<Guid?> GetUserOrganizationAsync(Guid userId);
}
