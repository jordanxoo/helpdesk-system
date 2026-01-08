using MassTransit;
using Shared.Events;
using Shared.DTOs;
using NotificationService.Services;
using Shared.HttpClients;

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
            var emailBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f59e0b; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ background-color: #f9fafb; padding: 30px; border: 1px solid #e5e7eb; }}
                        .warning {{ background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0; }}
                        .button {{ display: inline-block; background-color: #3b82f6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>⏰ Przypomnienie o Tickecie</h1>
                        </div>
                        <div class='content'>
                            <p>Witaj <strong>{customer.FirstName}</strong>!</p>
                            
                            <div class='warning'>
                                <h3>🎫 {reminder.Title}</h3>
                                <p>Twój ticket oczekuje na odpowiedź już <strong>{reminder.HoursSinceCreated} godzin</strong> ({reminder.HoursSinceCreated / 24:F1} dni).</p>
                            </div>
                            
                            <p>Ticket ID: <strong>#{reminder.TicketId}</strong></p>
                            <p>Status: <strong>Open</strong></p>
                            <p>Utworzony: <strong>{reminder.HoursSinceCreated}h temu</strong></p>
                            
                            <p>Prosimy o:</p>
                            <ul>
                                <li>Dodanie komentarza z aktualizacją</li>
                                <li>Sprawdzenie czy ticket nadal jest aktualny</li>
                                <li>Zamknięcie ticketa jeśli problem został rozwiązany</li>
                            </ul>
                            
                            <a href='http://localhost:5173/tickets/{reminder.TicketId}' class='button'>
                                Zobacz Ticket
                            </a>
                            
                            <div class='footer'>
                                <p>To jest automatyczne przypomnienie wysłane przez system Helpdesk.</p>
                                <p>Jeśli ticket nie jest już aktualny, możesz go zamknąć.</p>
                            </div>
                        </div>
                    </div>
                </body>
                </html>
            ";

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
                    await _emailService.SendEmailAsync(
                        to: agent.Email,
                        subject: $"⏰ Przypomnienie: Ticket #{reminder.TicketId} wymaga uwagi",
                        body: $@"
                            <p>Witaj {agent.FirstName}!</p>
                            <p>Ticket <strong>{reminder.Title}</strong> jest przypisany do Ciebie i nie ma aktywności od {reminder.HoursSinceCreated} godzin.</p>
                            <p><a href='http://localhost:5173/tickets/{reminder.TicketId}'>Zobacz ticket</a></p>
                        ",
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
