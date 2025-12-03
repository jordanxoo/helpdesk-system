using MassTransit;
using Shared.Events;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class UserLoggedInConsumer : IConsumer<UserLoggedInEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserLoggedInConsumer> _logger;

    public UserLoggedInConsumer(IEmailService emailService, ILogger<UserLoggedInConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserLoggedInEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "MassTransit: Processing UserLoggedInEvent: UserId={UserId}, Email={Email}", 
            message.UserId, 
            message.Email);

        try
        {
            await _emailService.SendLoginEmailAsync(message.Email);
            _logger.LogInformation("UserLoggedInEvent processed successfully for user {UserId}", message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process UserLoggedInEvent for user {UserId}", message.UserId);
            throw; // Rzucamy wyjątek aby MassTransit mógł obsłużyć retry logic
        }
    }
}
