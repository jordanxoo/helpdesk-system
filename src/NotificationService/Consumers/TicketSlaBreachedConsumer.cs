using MassTransit;
using Shared.Events;
using Shared.DTOs;
using NotificationService.Services;
using Shared.HttpClients;

namespace NotificationService.Consumers;

public class TicketSlaBreachedConsumer : IConsumer<TicketSlaBreachedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ISignalRNotificationService _signalRService;
    private readonly ILogger<TicketSlaBreachedConsumer> _logger;
    private readonly IUserServiceClient _userServiceClient;

    public TicketSlaBreachedConsumer(
        IEmailService emailService,
        ISignalRNotificationService signalRService,
        ILogger<TicketSlaBreachedConsumer> logger,
        IUserServiceClient userServiceClient)
    {
        _emailService = emailService;
        _signalRService = signalRService;
        _logger = logger;
        _userServiceClient = userServiceClient;
    }

    public async Task Consume(ConsumeContext<TicketSlaBreachedEvent> context)
    {
        var slaEvent = context.Message;
        
        _logger.LogInformation("Processing TicketSlaBreachedEvent for ticket {TicketId} ({Hours}h overdue)",
            slaEvent.TicketId, slaEvent.HoursOverdue);

        try
        {
            // Pobierz dane customera
            var customer = await _userServiceClient.GetUserAsync(slaEvent.CustomerId);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found for ticket {TicketId}",
                    slaEvent.CustomerId, slaEvent.TicketId);
                return;
            }

            // Email do customera o naruszeniu SLA
            var customerEmailBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #ef4444; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ background-color: #f9fafb; padding: 30px; border: 1px solid #e5e7eb; }}
                        .critical {{ background-color: #fee2e2; border-left: 4px solid #ef4444; padding: 15px; margin: 20px 0; }}
                        .button {{ display: inline-block; background-color: #ef4444; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .info {{ background-color: #dbeafe; border-left: 4px solid #3b82f6; padding: 15px; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🚨 SLA BREACH - Priorytet Krytyczny</h1>
                        </div>
                        <div class='content'>
                            <p>Witaj <strong>{customer.FirstName}</strong>!</p>
                            
                            <div class='critical'>
                                <h3>⚠️ {slaEvent.Title}</h3>
                                <p>Twój ticket <strong>przekroczył limit czasu SLA</strong> i został automatycznie eskalowany do <strong>CRITICAL</strong>.</p>
                            </div>
                            
                            <div class='info'>
                                <p><strong>Szczegóły:</strong></p>
                                <ul>
                                    <li>Ticket ID: <strong>#{slaEvent.TicketId}</strong></li>
                                    <li>Czas od utworzenia: <strong>{slaEvent.HoursOverdue} godzin ({slaEvent.HoursOverdue / 24:F1} dni)</strong></li>
                                    <li>Utworzony: <strong>{slaEvent.CreatedAt:dd.MM.yyyy HH:mm}</strong></li>
                                    <li>Nowy priorytet: <strong>CRITICAL</strong></li>
                                </ul>
                            </div>
                            
                            <p>Nasz zespół został powiadomiony o tym tickecie i nadano mu najwyższy priorytet.</p>
                            <p>Skontaktujemy się z Tobą w najkrótszym możliwym czasie.</p>
                            
                            <a href='http://localhost:5173/tickets/{slaEvent.TicketId}' class='button'>
                                Zobacz Ticket
                            </a>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: $"🚨 SLA BREACH - Ticket #{slaEvent.TicketId} eskalowany do CRITICAL",
                body: customerEmailBody,
                isHtml: true
            );

            // SignalR notification do customera
            await _signalRService.SendNotificationToUser(
                slaEvent.CustomerId.ToString(),
                new TicketSlaBreachedNotification
                {
                    TicketId = slaEvent.TicketId,
                    Title = slaEvent.Title,
                    Message = $"Ticket '{slaEvent.Title}' został eskalowany do CRITICAL ({slaEvent.HoursOverdue}h overdue)",
                    Timestamp = slaEvent.Timestamp,
                    ActionUrl = $"/tickets/{slaEvent.TicketId}",
                    ShowToast = true,
                    HoursOverdue = slaEvent.HoursOverdue,
                    DaysOverdue = slaEvent.HoursOverdue / 24,
                    CustomerId = slaEvent.CustomerId,
                    AgentId = slaEvent.AgentId,
                    CreatedAt = slaEvent.CreatedAt
                }
            );

            // Email do agenta (jeśli jest przypisany)
            if (slaEvent.AgentId.HasValue)
            {
                var agent = await _userServiceClient.GetUserAsync(slaEvent.AgentId.Value);
                if (agent != null)
                {
                    var agentEmailBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #ef4444;'>🚨 URGENT: SLA Breach Alert</h2>
                            <p>Witaj <strong>{agent.FirstName}</strong>!</p>
                            
                            <div style='background-color: #fee2e2; border-left: 4px solid #ef4444; padding: 15px; margin: 20px 0;'>
                                <h3>{slaEvent.Title}</h3>
                                <p>Ticket przypisany do Ciebie <strong>przekroczył SLA</strong> i został automatycznie eskalowany do <strong>CRITICAL</strong>.</p>
                            </div>
                            
                            <p><strong>Szczegóły:</strong></p>
                            <ul>
                                <li>Ticket ID: #{slaEvent.TicketId}</li>
                                <li>Czas overdue: {slaEvent.HoursOverdue}h ({slaEvent.HoursOverdue / 24:F1} dni)</li>
                                <li>Customer: {customer.FullName}</li>
                                <li>Nowy priorytet: CRITICAL</li>
                            </ul>
                            
                            <p style='color: #ef4444; font-weight: bold;'>⚠️ Wymagana natychmiastowa akcja!</p>
                            
                            <a href='http://localhost:5173/tickets/{slaEvent.TicketId}' 
                               style='display: inline-block; background-color: #ef4444; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px;'>
                                Akcja Natychmiast
                            </a>
                        </body>
                        </html>
                    ";

                    await _emailService.SendEmailAsync(
                        to: agent.Email,
                        subject: $"🚨 URGENT: SLA BREACH - Ticket #{slaEvent.TicketId} wymaga natychmiastowej akcji",
                        body: agentEmailBody,
                        isHtml: true
                    );

                    // SignalR notification do agenta
                    await _signalRService.SendNotificationToUser(
                        slaEvent.AgentId.Value.ToString(),
                        new TicketSlaBreachedNotification
                        {
                            TicketId = slaEvent.TicketId,
                            Title = slaEvent.Title,
                            Message = $"Ticket '{slaEvent.Title}' przekroczył SLA ({slaEvent.HoursOverdue}h) - CRITICAL!",
                            ActionUrl = $"/tickets/{slaEvent.TicketId}",
                            ShowToast = true,
                            HoursOverdue = slaEvent.HoursOverdue,
                            DaysOverdue = slaEvent.HoursOverdue / 24,
                            CustomerId = slaEvent.CustomerId,
                            AgentId = slaEvent.AgentId,
                            CreatedAt = slaEvent.CreatedAt,
                            Timestamp = slaEvent.Timestamp
                        }
                    );

                    _logger.LogWarning("Sent SLA breach alert to agent {AgentEmail} for ticket {TicketId}",
                        agent.Email, slaEvent.TicketId);
                }
            }

            _logger.LogWarning("Sent SLA breach notification for ticket {TicketId} to customer {Email} ({Hours}h overdue)",
                slaEvent.TicketId, customer.Email, slaEvent.HoursOverdue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TicketSlaBreachedEvent for ticket {TicketId}",
                slaEvent.TicketId);
            throw;
        }
    }
}
