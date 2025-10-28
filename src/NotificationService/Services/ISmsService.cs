namespace NotificationService.Services;

/// <summary>
/// Service for sending SMS notifications.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send SMS notification.
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="message">SMS message content</param>
    Task SendSmsAsync(string phoneNumber, string message);
    
    /// <summary>
    /// Send ticket created SMS notification.
    /// </summary>
    Task SendTicketCreatedSmsAsync(string phoneNumber, string ticketId);
}
