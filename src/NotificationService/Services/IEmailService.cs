namespace NotificationService.Services;

/// <summary>
/// Service for sending email notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email notification.
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML supported)</param>
    /// <param name="isHtml">Whether body is HTML</param>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    
    /// <summary>
    /// Send ticket created notification.
    /// </summary>
    Task SendTicketCreatedNotificationAsync(string customerEmail, string ticketId, string title);
    
    /// <summary>
    /// Send ticket assigned notification.
    /// </summary>
    Task SendTicketAssignedNotificationAsync(string agentEmail, string ticketId, string title);
    
    /// <summary>
    /// Send ticket status changed notification.
    /// </summary>
    Task SendTicketStatusChangedNotificationAsync(string customerEmail, string ticketId, string newStatus);
    
    /// <summary>
    /// Send new comment notification.
    /// </summary>
    Task SendNewCommentNotificationAsync(string recipientEmail, string ticketId, string commentContent);

    Task SendWelcomeEmailAsync(string email, string firstName);
    
    Task SendLoginEmailAsync(string email);
    Task SendTicketStatusChangedEmailAsync(string email,string ticketId,string Title);
}
