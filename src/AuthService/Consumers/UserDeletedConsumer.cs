using MassTransit;
using Microsoft.AspNetCore.Identity;
using Shared.Events;
using AuthService.Data;

namespace AuthService.Consumers;

public class UserDeletedConsumer : IConsumer<UserDeletedEvent>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserDeletedConsumer> _logger;

    public UserDeletedConsumer(UserManager<ApplicationUser> userManager, ILogger<UserDeletedConsumer> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received UserDeletedEvent for UserId: {UserId}, Email: {Email}", 
            message.UserId, message.Email);

        try
        {
            var user = await _userManager.FindByIdAsync(message.UserId.ToString());
            
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found in AuthService, skipping deletion", message.UserId);
                return;
            }

            var result = await _userManager.DeleteAsync(user);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to delete user {UserId} from AuthService: {Errors}", 
                    message.UserId, errors);
                throw new InvalidOperationException($"Failed to delete user from AuthService: {errors}");
            }
            
            _logger.LogInformation("User {UserId} ({Email}) successfully deleted from AuthService", 
                message.UserId, message.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId} from AuthService", message.UserId);
            throw; // Let MassTransit handle retry
        }
    }
}
