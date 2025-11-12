using Shared.Constants;
using Shared.Events;
using Shared.Messaging;
using UserService.Services;

namespace UserService.Workers;

public class UserEventConsumer : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserEventConsumer> _logger;

    public UserEventConsumer(
        IMessageConsumer messageConsumer,
        IServiceProvider serviceProvider,
        ILogger<UserEventConsumer> logger)
    {
        _messageConsumer = messageConsumer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserEventConsumer starting...");

        // Wait for RabbitMQ to be ready
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        try
        {
            // Start both consumers concurrently
            await Task.WhenAll(
                ConsumeUserRegisteredEventsAsync(stoppingToken),
                ConsumeProfileUpdatedEventsAsync(stoppingToken)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UserEventConsumer encountered an error");
            throw;
        }
    }

    private async Task ConsumeUserRegisteredEventsAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.SubscribeAsync<UserRegisteredEvent>(
            queueName: RoutingKeys.UserRegistered,
            handler: async (userEvent) => await HandleUserRegisteredAsync(userEvent),
            cancellationToken: stoppingToken);
    }

    private async Task ConsumeProfileUpdatedEventsAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.SubscribeAsync<ProfileUpdatedEvent>(
            queueName: RoutingKeys.ProfileUpdated,
            handler: async (profileEvent) => await HandleProfileUpdatedAsync(profileEvent),
            cancellationToken: stoppingToken);
    }

    private async Task<bool> HandleUserRegisteredAsync(UserRegisteredEvent userEvent)
    {
        _logger.LogInformation("Processing UserRegisteredEvent for user: {Email} (ID: {UserId})", 
            userEvent.Email, userEvent.UserId);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            // Sprawdź czy user już istnieje (idempotencja)
            var existingUser = await userService.GetByIdAsync(userEvent.UserId);
            if (existingUser != null)
            {
                _logger.LogInformation("User already exists in UserService: {Email}", userEvent.Email);
                return true;
            }

            // Utwórz użytkownika w UserService
            var createRequest = new Shared.DTOs.CreateUserRequest(
                Email: userEvent.Email,
                FirstName: userEvent.FirstName,
                LastName: userEvent.LastName,
                PhoneNumber: string.Empty,
                Role: userEvent.Role
            );

            await userService.CreateAsync(createRequest, userEvent.UserId);

            _logger.LogInformation("User created successfully in UserService: {Email}", userEvent.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserRegisteredEvent for user: {Email}", userEvent.Email);
            return false;
        }
    }

    private async Task<bool> HandleProfileUpdatedAsync(ProfileUpdatedEvent profileEvent)
    {
        _logger.LogInformation("Processing ProfileUpdatedEvent for user: {UserId}", profileEvent.UserId);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            // Get existing user
            var user = await userService.GetByIdAsync(profileEvent.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found in UserService during profile sync: {UserId}", profileEvent.UserId);
                return false;
            }

            // Sync changes from AuthService (source of truth)
            var repository = scope.ServiceProvider.GetRequiredService<Repositories.IUserRepository>();
            var userEntity = await repository.GetByIdAsync(profileEvent.UserId);
            
            if (userEntity == null)
            {
                _logger.LogError("User entity not found: {UserId}", profileEvent.UserId);
                return false;
            }

            var hasChanges = false;

            if (!string.IsNullOrWhiteSpace(profileEvent.FirstName) && userEntity.FirstName != profileEvent.FirstName)
            {
                userEntity.FirstName = profileEvent.FirstName;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(profileEvent.LastName) && userEntity.LastName != profileEvent.LastName)
            {
                userEntity.LastName = profileEvent.LastName;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(profileEvent.PhoneNumber) && userEntity.PhoneNumber != profileEvent.PhoneNumber)
            {
                userEntity.PhoneNumber = profileEvent.PhoneNumber;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(profileEvent.Role))
            {
                if (Enum.TryParse<Shared.Models.UserRole>(profileEvent.Role, out var newRole) && userEntity.Role != newRole)
                {
                    userEntity.Role = newRole;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                userEntity.UpdatedAt = DateTime.UtcNow;
                await repository.UpdateAsync(userEntity);
                _logger.LogInformation("User profile synced successfully in UserService: {UserId}", profileEvent.UserId);
            }
            else
            {
                _logger.LogInformation("No changes to sync for user: {UserId}", profileEvent.UserId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle ProfileUpdatedEvent for user: {UserId}", profileEvent.UserId);
            return false;
        }
    }
}
