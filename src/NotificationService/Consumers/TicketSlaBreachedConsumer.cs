using MassTransit;
using Shared.Events;
using Shared.DTOs;
using NotificationService.Services;
using Shared.HttpClients;
using NotificationService.Templates;

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
            var customerEmailBody = EmailTemplates.SlaBreached(
                firstName: customer.FirstName,
                title: slaEvent.Title,
                ticketId: slaEvent.TicketId,
                hoursOverdue: slaEvent.HoursOverdue,
                createdAt: slaEvent.CreatedAt
            );

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
                    var agentEmailBody = EmailTemplates.SlaBreachedAgent(
                        firstName: agent.FirstName,
                        title: slaEvent.Title,
                        ticketId: slaEvent.TicketId,
                        hoursOverdue: slaEvent.HoursOverdue,
                        customerName: customer.FullName
                    );

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
