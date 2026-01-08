using MassTransit;
using Shared.Events;
using Shared.HttpClients;
using Shared.DTOs;
using NotificationService.Services;
using NotificationService.Templates;

namespace NotificationService.Consumers;

/// <summary>
/// Consumer dla TicketStatusChangedEvent.
/// Powiadamia klienta (i opcjonalnie agenta) o zmianie statusu ticketu.
/// </summary>
public class TicketStatusChangedConsumer : IConsumer<TicketStatusChangedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ISignalRNotificationService _signalRService;
    private readonly IUserServiceClient _userServiceClient;
    private readonly ILogger<TicketStatusChangedConsumer> _logger;

    public TicketStatusChangedConsumer(
        IEmailService emailService,
        ISignalRNotificationService signalRService,
        IUserServiceClient userServiceClient,
        ILogger<TicketStatusChangedConsumer> logger)
    {
        _emailService = emailService;
        _signalRService = signalRService;
        _userServiceClient = userServiceClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TicketStatusChangedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Processing TicketStatusChangedEvent: Ticket {TicketId} status changed from {OldStatus} to {NewStatus}",
            message.TicketId,
            message.OldStatus,
            message.NewStatus);

        try
        {
            var customer = await _userServiceClient.GetUserAsync(message.CustomerId);

            if (customer == null)
            {
                _logger.LogWarning(
                    "Customer {CustomerId} not found for ticket {TicketId}",
                    message.CustomerId,
                    message.TicketId);
                return;
            }

            // Określ priorytet na podstawie nowego statusu
            var priority = DeterminePriority(message.NewStatus);

            // Stwórz przyjazną wiadomość dla użytkownika
            var userFriendlyMessage = GetStatusChangeMessage(
                message.TicketId,
                message.OldStatus,
                message.NewStatus,
                customer.FirstName);

            var notification = new TicketStatusChangedNotification
            {
                TicketId = message.TicketId,
                OldStatus = message.OldStatus,
                NewStatus = message.NewStatus,
                Message = userFriendlyMessage,
                Timestamp = message.Timestamp,
                Priority = priority,
                ActionUrl = $"/tickets/{message.TicketId}",
                ShowToast = true
            };

            // Wyślij notyfikację do klienta
            await _signalRService.SendTicketUpdate(
                message.CustomerId.ToString(),
                notification);

            // Jeśli jest agent, wyślij też do niego
            if (message.AgentId.HasValue)
            {
                var agentNotification = new TicketStatusChangedNotification
                {
                    TicketId = message.TicketId,
                    OldStatus = message.OldStatus,
                    NewStatus = message.NewStatus,
                    Message = $"Status ticketu #{message.TicketId} zmieniony: {message.OldStatus} → {message.NewStatus}",
                    Timestamp = message.Timestamp,
                    Priority = "normal",
                    ActionUrl = $"/tickets/{message.TicketId}",
                    ShowToast = false // Agent nie potrzebuje toast przy każdej zmianie
                };

                await _signalRService.SendTicketUpdate(
                    message.AgentId.Value.ToString(),
                    agentNotification);
            }

            // Wyślij email przy ważnych zmianach statusu
            if (ShouldSendEmail(message.NewStatus))
            {
                await _emailService.SendTicketStatusChangedEmailAsync(
                    email: customer.Email,
                    firstName: customer.FirstName,
                    ticketId: message.TicketId.ToString(),
                    title: $"Ticket #{message.TicketId}",
                    oldStatus: message.OldStatus,
                    newStatus: message.NewStatus
                );
            }

            _logger.LogInformation(
                "Sent status change notification for ticket {TicketId} to customer {CustomerId}",
                message.TicketId,
                message.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing TicketStatusChangedEvent for ticket {TicketId}",
                message.TicketId);
            throw;
        }
    }

    private static string DeterminePriority(string newStatus)
    {
        return newStatus.ToLower() switch
        {
            "closed" => "high",      
            "resolved" => "high",   
            "in progress" => "normal",
            "open" => "low",
            _ => "normal"
        };
    }


    private static string GetStatusChangeMessage(
        Guid ticketId,
        string oldStatus,
        string newStatus,
        string customerFirstName)
    {
        return newStatus.ToLower() switch
        {
            "in progress" => $"Świetnie {customerFirstName}! Twój ticket #{ticketId} jest właśnie rozwiązywany.",
            "resolved" => $"Dobra wiadomość! Ticket #{ticketId} został rozwiązany. Sprawdź i potwierdź rozwiązanie.",
            "closed" => $"Ticket #{ticketId} został zamknięty. Dziękujemy za skorzystanie z helpdesku!",
            "on hold" => $"Ticket #{ticketId} został wstrzymany. Skontaktujemy się wkrótce.",
            _ => $"Status ticketu #{ticketId} zmienił się: {oldStatus} → {newStatus}"
        };
    }

      private static bool ShouldSendEmail(string newStatus)
    {
        return newStatus.ToLower() is "resolved" or "closed";
    }
}