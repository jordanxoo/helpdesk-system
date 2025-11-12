using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AuthService.Data;
using Shared.Constants;
using Shared.DTOs;
using Shared.Events;
using Shared.Messaging;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth/[controller]")]
[Authorize(Roles = UserRoles.Administrator)]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        IMessagePublisher messagePublisher,
        ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    /// <summary>
    /// Update user profile data (firstName, lastName, phoneNumber, role).
    /// AuthService is the source of truth - changes are synced to UserService via event.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UpdateUserProfileRequest request)
    {
        _logger.LogInformation("Admin updating profile for user: {UserId}", id);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return NotFound(new { message = "User not found" });
        }

        var hasChanges = false;

        // Update basic profile fields
        if (!string.IsNullOrWhiteSpace(request.FirstName) && user.FirstName != request.FirstName)
        {
            user.FirstName = request.FirstName;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName) && user.LastName != request.LastName)
        {
            user.LastName = request.LastName;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber)
        {
            user.PhoneNumber = request.PhoneNumber;
            hasChanges = true;
        }

        // Update role if changed
        string? newRole = null;
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var validRoles = new[] { UserRoles.Customer, UserRoles.Agent, UserRoles.Administrator };
            if (!validRoles.Contains(request.Role))
            {
                return BadRequest(new { message = $"Invalid role. Must be one of: {string.Join(", ", validRoles)}" });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(request.Role))
            {
                // Remove old roles and add new one
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, request.Role);
                newRole = request.Role;
                hasChanges = true;
                _logger.LogInformation("Changed role for user {UserId}: {OldRole} -> {NewRole}", 
                    id, string.Join(",", currentRoles), request.Role);
            }
        }

        if (!hasChanges)
        {
            return Ok(new { message = "No changes detected" });
        }

        // Save changes to AspNetUsers
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to update user {UserId}: {Errors}", 
                id, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(new { message = "Failed to update user", errors = result.Errors });
        }

        // Publish event for UserService to sync
        var profileEvent = new ProfileUpdatedEvent
        {
            UserId = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = newRole
        };

        await _messagePublisher.PublishAsync(profileEvent, RoutingKeys.ProfileUpdated);
        _logger.LogInformation("Published ProfileUpdatedEvent for user: {UserId}", id);

        return Ok(new { message = "User profile updated successfully" });
    }
}
