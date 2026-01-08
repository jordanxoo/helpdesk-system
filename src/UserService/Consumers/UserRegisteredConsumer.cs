using MassTransit;
using Shared.DTOs;
using Shared.Events;
using Shared.Models;
using UserService.Services;

namespace UserService.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;
    private readonly IUserService _userService;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }



    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received UserRegisteredEvent for {Email} with ID {userID}: ",message.Email,message.UserId);

        try
        {
            // Try to get existing user, but don't throw if not found
            UserDto? exsistingUser = null;
            try
            {
                exsistingUser = await _userService.GetByIdAsync(message.UserId);
            }
            catch (Shared.Exceptions.NotFoundException)
            {
                // User doesn't exist yet - this is expected for new registrations
                _logger.LogDebug("User {UserId} not found in UserService, will create new user", message.UserId);
            }
            
            if(exsistingUser != null)
            {
                _logger.LogWarning("User {userID} already exsists, skipping",message.UserId);
                return;
            }

            var createRequest = new CreateUserRequest(
                message.Email.ToString(),
                message.FirstName,
                message.LastName,
                message.PhoneNumber,
                message.Role.ToString()
            );


            await _userService.CreateAsync(createRequest, message.UserId);

            _logger.LogInformation("User {Email} created successfully in userService with id {userID}",message.Email,message.UserId);
        }catch(Exception ex)
        {
            _logger.LogError(ex,"Failed to create user {Email} in UserService",message.Email);
            throw;
        }
    }
}