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
            await ConsumeUserRegisteredEventsAsync(stoppingToken);
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
            queueName: Shared.Constants.RoutingKeys.UserRegistered,
            handler: async (userEvent) => await HandleUserRegisteredAsync(userEvent),
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
}
