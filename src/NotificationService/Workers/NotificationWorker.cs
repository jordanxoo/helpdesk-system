using System.Text.Json;
using NotificationService.Services;
using Shared.Constants;
using Shared.Events;
using Shared.Messaging;

namespace NotificationService.Workers;

public class NotificationWorker : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationWorker> _logger;

    public NotificationWorker(
        IMessageConsumer messageConsumer,
        IServiceProvider serviceProvider,
        ILogger<NotificationWorker> logger)
    {
        _messageConsumer = messageConsumer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationWorker starting...");

        try
        {
            var tasks = new List<Task>
            {
                ConsumeTicketCreatedEventsAsync(stoppingToken),
                ConsumeTicketAssignedEventsAsync(stoppingToken),
                ConsumeTicketStatusChangedEventsAsync(stoppingToken),
                ConsumeCommentAddedEventsAsync(stoppingToken)
            };

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NotificationWorker encountered an error");
            throw;
        }
    }

    private async Task ConsumeTicketCreatedEventsAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.SubscribeAsync<TicketCreatedEvent>(
            queueName: RoutingKeys.TicketCreated,
            handler: async (ticketEvent) => await HandleTicketCreatedAsync(ticketEvent),
            cancellationToken: stoppingToken);
    }

    private async Task ConsumeTicketAssignedEventsAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.SubscribeAsync<TicketAssignedEvent>(
            queueName: RoutingKeys.TicketAssigned,
            handler: async (assignedEvent) => await HandleTicketAssignedAsync(assignedEvent),
            cancellationToken: stoppingToken);
    }

    private async Task ConsumeTicketStatusChangedEventsAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.SubscribeAsync<TicketStatusChangedEvent>(
            queueName: RoutingKeys.TicketStatusChanged,
            handler: async (statusEvent) => await HandleTicketStatusChangedAsync(statusEvent),
            cancellationToken: stoppingToken);
    }

    private async Task ConsumeCommentAddedEventsAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.SubscribeAsync<CommentAddedEvent>(
            queueName: RoutingKeys.CommentAdded,
            handler: async (commentEvent) => await HandleCommentAddedAsync(commentEvent),
            cancellationToken: stoppingToken);
    }

    private async Task<bool> HandleTicketCreatedAsync(TicketCreatedEvent ticketEvent)
    {
        try
        {
            _logger.LogInformation(
                "Processing TicketCreatedEvent: TicketId={TicketId}, Customer={CustomerId}",
                ticketEvent.TicketId, ticketEvent.CustomerId);

            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // TODO: Fetch customer details from AuthService/UserService using CustomerId via HTTP client
            // For now, using mock data
            var customerEmail = $"customer-{ticketEvent.CustomerId}@example.com";
            var ticketTitle = "New Support Ticket";

            await emailService.SendTicketCreatedNotificationAsync(
                customerEmail,
                ticketEvent.TicketId.ToString(),
                ticketTitle);

            _logger.LogInformation("TicketCreatedEvent processed successfully for ticket {TicketId}", ticketEvent.TicketId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process TicketCreatedEvent for ticket {TicketId}", ticketEvent.TicketId);
            return false;
        }
    }

    private async Task<bool> HandleTicketAssignedAsync(TicketAssignedEvent assignedEvent)
    {
        try
        {
            _logger.LogInformation(
                "Processing TicketAssignedEvent: TicketId={TicketId}, Agent={AgentId}",
                assignedEvent.TicketId, assignedEvent.AgentId);

            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // TODO: Fetch agent details from UserService using AgentId via HTTP client
            // TODO: Fetch ticket details from TicketService using TicketId via HTTP client
            // For now, using mock data
            var agentEmail = $"agent-{assignedEvent.AgentId}@example.com";
            var ticketTitle = "Support Ticket Assignment";

            await emailService.SendTicketAssignedNotificationAsync(
                agentEmail,
                assignedEvent.TicketId.ToString(),
                ticketTitle);

            _logger.LogInformation("TicketAssignedEvent processed successfully for ticket {TicketId}", assignedEvent.TicketId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process TicketAssignedEvent for ticket {TicketId}", assignedEvent.TicketId);
            return false;
        }
    }

    private async Task<bool> HandleTicketStatusChangedAsync(TicketStatusChangedEvent statusEvent)
    {
        try
        {
            _logger.LogInformation(
                "Processing TicketStatusChangedEvent: TicketId={TicketId}, Status={OldStatus}->{NewStatus}",
                statusEvent.TicketId, statusEvent.OldStatus, statusEvent.NewStatus);

            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // TODO: Fetch customer details from AuthService/UserService using CustomerId via HTTP client
            // For now, using mock data
            var customerEmail = $"customer-{statusEvent.CustomerId}@example.com";

            await emailService.SendTicketStatusChangedNotificationAsync(
                customerEmail,
                statusEvent.TicketId.ToString(),
                statusEvent.NewStatus);

            _logger.LogInformation("TicketStatusChangedEvent processed successfully for ticket {TicketId}", statusEvent.TicketId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process TicketStatusChangedEvent for ticket {TicketId}", statusEvent.TicketId);
            return false;
        }
    }

    private async Task<bool> HandleCommentAddedAsync(CommentAddedEvent commentEvent)
    {
        try
        {
            _logger.LogInformation(
                "Processing CommentAddedEvent: TicketId={TicketId}, Comment={CommentId}",
                commentEvent.TicketId, commentEvent.CommentId);

            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // TODO: Fetch customer details from AuthService/UserService using CustomerId via HTTP client
            // For now, using mock data
            var recipientEmail = $"customer-{commentEvent.CustomerId}@example.com";

            await emailService.SendNewCommentNotificationAsync(
                recipientEmail,
                commentEvent.TicketId.ToString(),
                commentEvent.Content);

            _logger.LogInformation("CommentAddedEvent processed successfully for ticket {TicketId}", commentEvent.TicketId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process CommentAddedEvent for ticket {TicketId}", commentEvent.TicketId);
            return false;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NotificationWorker stopping...");
        await _messageConsumer.StopAsync();
        await base.StopAsync(cancellationToken);
    }
}
