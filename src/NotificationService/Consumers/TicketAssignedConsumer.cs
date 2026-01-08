using MassTransit;
using Shared.Events;
using Shared.HttpClients;
using Shared.DTOs;
using NotificationService.Services;
using Shared.Models;
using NotificationService.Templates;

namespace NotificationService.Consumers;


public class TicketAssignedConsumer : IConsumer<TicketAssignedEvent>
{
    private readonly ILogger<TicketAssignedConsumer> _logger;
    private readonly ISignalRNotificationService _signalRService;
    private readonly IEmailService _emailService;
    private readonly IUserServiceClient _userServiceClient;

    public TicketAssignedConsumer(ILogger<TicketAssignedConsumer> logger,ISignalRNotificationService signalRNotification,
     IEmailService emailService,IUserServiceClient userServiceClient)
    {
        _logger = logger;  
        _signalRService = signalRNotification;
        _emailService = emailService;
        _userServiceClient =userServiceClient;
    }

    public async Task Consume(ConsumeContext<TicketAssignedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing TicketAssignedEvenet: ticket {ticketId} assigned to agent {agentId}",message.TicketId,message.TicketId);


        try
        {
            var customerTask = _userServiceClient.GetUserAsync(message.CustomerId);
            var agentTask = _userServiceClient.GetUserAsync(message.AgentId);

            await Task.WhenAll(customerTask,agentTask);
        
            var customer = await customerTask;
            var agent = await agentTask;

            if(customer == null)
            {
                _logger.LogWarning("Customer {customerId} not found for ticket {ticketId}",message.CustomerId,message.TicketId);
                return;
            }

            if(agent == null)
            {
                _logger.LogWarning("Agent {agentId} not found for ticket {ticketId}",message.AgentId,message.TicketId);
                return;
            }

            var customerNotification = new TicketAssignedNotification
            {
                TicketId = message.TicketId,
                AgentId = message.AgentId,
                AgentName = agent.FullName,
                CustomerId = message.CustomerId,
                Message = $"Twój ticket #{message.TicketId} został przypisany do agenta {agent.FullName}",
                Timestamp = message.Timestamp,
                Priority = "normal",
                ActionUrl = $"/tickets/{message.TicketId}",
                ShowToast = true
            };

            await _signalRService.SendNotificationToUser(message.CustomerId.ToString(),customerNotification);

            // Wyślij email do customera
            var customerEmailBody = EmailTemplates.TicketAssigned(
                firstName: customer.FirstName,
                ticketId: message.TicketId,
                title: $"Ticket #{message.TicketId}",
                agentName: agent.FullName
            );

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: $"👤 Ticket #{message.TicketId} przypisany do agenta",
                body: customerEmailBody,
                isHtml: true
            );

            _logger.LogInformation("Sent notification to customer {customerId} about ticket assignment",message.TicketId);

            var agentNotification = new TicketAssignedNotification
            {
                TicketId = message.TicketId,
                AgentId = message.AgentId,
                AgentName = agent.FullName,
                CustomerId = message.CustomerId,
                Message = $"Przypisano ci nowy ticket #{message.TicketId} od klienta #{customer.FullName}",
                Timestamp = message.Timestamp,
                Priority = "high",
                ActionUrl = $"/tickets/{message.TicketId}",
                ShowToast = true,
                Metadata = new Dictionary<string, object>
                {
                    {"CustomerName", customer.FullName},
                    {"CustomerEmail",customer.Email}
                }
            };

            await _signalRService.SendNotificationToUser(message.AgentId.ToString(),
            agentNotification);

            // Wyślij email do agenta
            var agentEmailBody = EmailTemplates.TicketAssignedAgent(
                firstName: agent.FirstName,
                ticketId: message.TicketId,
                title: $"Ticket #{message.TicketId}",
                customerName: customer.FullName,
                priority: "Normal"
            );

            await _emailService.SendEmailAsync(
                to: agent.Email,
                subject: $"📥 Nowy ticket przypisany: #{message.TicketId}",
                body: agentEmailBody,
                isHtml: true
            );

            _logger.LogInformation("Sent notification to agent {agentId} about new assigned Ticket {ticketid}",
            message.AgentId,message.TicketId);
        
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error processing TicketAssignedEvent for ticket {ticketId}",message.TicketId);
            throw;
        }
    }
}