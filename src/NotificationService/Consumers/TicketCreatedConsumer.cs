using MassTransit;
using Shared.Events;
using NotificationService.Services;
using Shared.HttpClients;
using Shared.DTOs;

namespace NotificationService.Consumers;


public class TicketCreatedConsumer : IConsumer<TicketCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ISignalRNotificationService _signalRService;
    private readonly ILogger<TicketCreatedConsumer> _logger;
    private readonly IUserServiceClient _userClientService;
    
    public TicketCreatedConsumer(
        IEmailService emailService,
        ISignalRNotificationService signalRService,
        ILogger<TicketCreatedConsumer> logger,
        IUserServiceClient userClientService
    )
    {
        _logger = logger;
        _emailService = emailService;
        _signalRService = signalRService;
        _userClientService = userClientService; 
    }


    public async Task Consume(ConsumeContext<TicketCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("MassTransit: Odebrano ticketCreatedEvent dla ID {ticketID}",message.TicketId);

        try
        {
            var customer = await _userClientService.GetUserAsync(message.CustomerId);

            if(customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found in UserService for the ticket {TicketId}. Using fallback email."
                ,message.CustomerId,message.TicketId);

                return;
            }
            await _emailService.SendTicketCreatedNotificationAsync(
                customer.Email,message.TicketId.ToString(),
                $"Nowe zgłoszenie #{message.TicketId}"
            );

            var notification = new TicketCreatedNotification
            {
                TicketId = message.TicketId,
                CustomerId = message.CustomerId,
                CustomerName = customer.FullName,
                Title = $"Nowe zgłoszenie #{message.TicketId}",
                Message = $"Witaj {customer.FirstName}! Twój ticket został utworzony i oczekuje na przypisanie.",
                Timestamp = message.Timestamp,
                Priority = "normal",
                ActionUrl = $"/tickets/{message.TicketId}",
                ShowToast = true
            };

            await _signalRService.SendNotificationToUser(message.CustomerId.ToString(),notification);

            _logger.LogInformation("Sent notifications for ticket {ticketId} to Email {email}",message.TicketId,customer.Email);
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error processing TicketCreatedEvent for ticket {ticketId}",message.TicketId);
            throw;
        }
    }
}
