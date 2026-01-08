using MassTransit;
using Shared.Events;
using Shared.DTOs;
using NotificationService.Services;
using Shared.HttpClients;
using NotificationService.Templates;

namespace NotificationService.Consumers;

public class TicketReminderConsumer : IConsumer<TicketReminderEvent>
{
    private readonly IEmailService _emailService;
    private readonly ISignalRNotificationService _signalRService;
    private readonly ILogger<TicketReminderConsumer> _logger;
    private readonly IUserServiceClient _userServiceClient;

    public TicketReminderConsumer(
        IEmailService emailService,
        ISignalRNotificationService signalRService,
        ILogger<TicketReminderConsumer> logger,
        IUserServiceClient userServiceClient)
    {
        _emailService = emailService;
        _signalRService = signalRService;
        _logger = logger;
        _userServiceClient = userServiceClient;
    }

    public async Task Consume(ConsumeContext<TicketReminderEvent> context)
    {
        var reminder = context.Message;
        
        _logger.LogInformation("Processing TicketReminderEvent for ticket {TicketId} (no activity for {Hours}h)",
            reminder.TicketId, reminder.HoursSinceCreated);

        try
        {
            // Pobierz dane customera
            var customer = await _userServiceClient.GetUserAsync(reminder.CustomerId);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found for ticket {TicketId}",
                    reminder.CustomerId, reminder.TicketId);
                return;
            }

            // Wysłanie EMAIL przypomnienia
            var emailBody = EmailTemplates.TicketReminder(
                firstName: customer.FirstName,
                title: reminder.Title,
                ticketId: reminder.TicketId,
                hoursSinceCreated: reminder.HoursSinceCreated,
                daysSinceCreated: reminder.HoursSinceCreated / 24.0
            );

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: $"⏰ Przypomnienie: Ticket #{reminder.TicketId} - {reminder.Title}",
                body: emailBody,
                isHtml: true
            );

            // SignalR Real-time Notification
            var notification = new TicketReminderNotification
            {
                TicketId = reminder.TicketId,
                Title = reminder.Title,
                Message = $"Ticket '{reminder.Title}' oczekuje już {reminder.HoursSinceCreated}h bez aktywności",
                Timestamp = reminder.Timestamp,
                ActionUrl = $"/tickets/{reminder.TicketId}",
                ShowToast = true,
                HoursSinceCreated = reminder.HoursSinceCreated,
                DaysSinceCreated = reminder.HoursSinceCreated / 24,
                CustomerId = reminder.CustomerId,
                AgentId = reminder.AgentId
            };

            await _signalRService.SendNotificationToUser(
                reminder.CustomerId.ToString(), 
                notification
            );

            // Jeśli jest przypisany agent, wyślij też do niego
            if (reminder.AgentId.HasValue)
            {
                var agent = await _userServiceClient.GetUserAsync(reminder.AgentId.Value);
                if (agent != null)
                {
                    var agentEmailBody = EmailTemplates.TicketReminderAgent(
                        firstName: agent.FirstName,
                        title: reminder.Title,
                        ticketId: reminder.TicketId,
                        hoursSinceCreated: reminder.HoursSinceCreated,
                        customerName: customer.FullName
                    );

                    await _emailService.SendEmailAsync(
                        to: agent.Email,
                        subject: $"⏰ Przypomnienie: Ticket #{reminder.TicketId} wymaga uwagi",
                        body: agentEmailBody,
                        isHtml: true
                    );

                    await _signalRService.SendNotificationToUser(
                        reminder.AgentId.Value.ToString(),
                        new TicketReminderNotification
                        {
                            TicketId = reminder.TicketId,
                            Title = reminder.Title,
                            Message = $"Ticket '{reminder.Title}' wymaga uwagi ({reminder.HoursSinceCreated}h bez aktywności)",
                            ActionUrl = $"/tickets/{reminder.TicketId}",
                            HoursSinceCreated = reminder.HoursSinceCreated,
                            DaysSinceCreated = reminder.HoursSinceCreated / 24,
                            CustomerId = reminder.CustomerId,
                            AgentId = reminder.AgentId,
                            Timestamp = reminder.Timestamp
                        }
                    );

                    _logger.LogInformation("Sent reminder to agent {AgentId} for ticket {TicketId}",
                        reminder.AgentId, reminder.TicketId);
                }
            }

            _logger.LogInformation("Sent reminder for ticket {TicketId} to customer {Email} ({Hours}h inactive)",
                reminder.TicketId, customer.Email, reminder.HoursSinceCreated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TicketReminderEvent for ticket {TicketId}",
                reminder.TicketId);
            throw;
        }
    }
}
