using MassTransit;
using Shared.Events;
using NotificationService.Services;
namespace NotificationService.Consumers;


public class TicketCreatedConsumer : IConsumer<TicketCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TicketCreatedConsumer> _logger;

    public TicketCreatedConsumer(IEmailService emailService, ILogger<TicketCreatedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<TicketCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("MassTransit: Odebrano TicketCreatedEvent dla ID {TicketID}", message.TicketId);
        // TODO: Tutaj w przyszłości do pobrania email z UserService przez HTTP
        // Na razie mock dla testów Mailpit
        var customerEmail = "klient@mailpit.local";
        var title = "Nowe zgłoszenie (MassTransit)";


        await _emailService.SendTicketCreatedNotificationAsync(customerEmail,message.TicketId.ToString(),title);
    }
}