using MassTransit;
using Shared.Events;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(IEmailService emailService, ILogger<UserRegisteredConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "MassTransit: Processing UserRegisteredEvent: UserId={UserId}, Email={Email}",
            message.UserId, message.Email);

        try
        {
            // wysylanie email powitalny
            await _emailService.SendWelcomeEmailAsync(message.Email, message.FirstName);
            
            _logger.LogInformation("UserRegisteredEvent processed successfully for user {UserId}", message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process UserRegisteredEvent for user {UserId}", message.UserId);
            // Rzucenie wyjątku spowoduje, że MassTransit spróbuje ponowić przetworzenie wiadomości (Retry Policy)
            throw; 
        }
    }
}