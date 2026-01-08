using System.Net;
using System.Net.Mail;
using System.Web;
using Microsoft.Extensions.Options;
using NotificationService.Configuration;
using NotificationService.Templates;

namespace NotificationService.Services;

/// <summary>
/// Email service implementation using SMTP.
/// For production, consider using AWS SES, SendGrid, or similar services.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            _logger.LogInformation("Sending email to {To} via {Host}:{Port}",to,_settings.SmtpServer,_settings.SmtpPort);
            
            using var client = new SmtpClient(_settings.SmtpServer,_settings.SmtpPort);

            if(!string.IsNullOrEmpty(_settings.SmtpPassword))
            {
                client.Credentials = new NetworkCredential(_settings.SenderEmail,_settings.SmtpPassword);
                client.EnableSsl = true;
            }
            else
            {
                // dla malpit/localhost bez ssl
                client.EnableSsl = false;
            }

            var MailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail,_settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            MailMessage.To.Add(to);

            await client.SendMailAsync(MailMessage);
            _logger.LogInformation("Email sent successfully to {To}",to);

        }catch(Exception ex)
        {
            _logger.LogError(ex,"Failed to send email to: {To}", to);
        }
    }

    public async Task SendTicketCreatedNotificationAsync(string customerEmail, string firstName, string ticketId, string title)
    {
        var subject = $"✅ Ticket został utworzony: {title}";

        var body = EmailTemplates.TicketCreated(
            firstName: firstName,
            ticketId: Guid.Parse(ticketId),
            title: title,
            priority: "Normal", // Domyślny priorytet
            category: "Support" // Domyślna kategoria
        );

        await SendEmailAsync(customerEmail, subject, body, isHtml: true);
    }

    public async Task SendTicketAssignedNotificationAsync(string agentEmail, string ticketId, string title)
    {
        var subject = $"New Ticket Assigned: {title}";
        var body = $@"
            <h2>A New Ticket Has Been Assigned to You</h2>
            <p><strong>Ticket ID:</strong> {HttpUtility.HtmlEncode(ticketId)}</p>
            <p><strong>Title:</strong> {HttpUtility.HtmlEncode(title)}</p>
            <p>Please review the ticket details and respond to the customer.</p>
            <br/>
            <p>Best regards,<br/>Helpdesk System</p>
        ";

        await SendEmailAsync(agentEmail, subject, body);
    }

    public async Task SendTicketStatusChangedNotificationAsync(string customerEmail, string ticketId, string newStatus)
    {
        var subject = $"Ticket Status Updated - {ticketId}";
        var body = $@"
            <h2>Your Ticket Status Has Changed</h2>
            <p><strong>Ticket ID:</strong> {HttpUtility.HtmlEncode(ticketId)}</p>
            <p><strong>New Status:</strong> {HttpUtility.HtmlEncode(newStatus)}</p>
            <p>You can view the details in your dashboard.</p>
            <br/>
            <p>Best regards,<br/>Helpdesk Support Team</p>
        ";

        await SendEmailAsync(customerEmail, subject, body);
    }

    public async Task SendNewCommentNotificationAsync(string recipientEmail, string ticketId, string commentContent)
    {
        var subject = $"New Comment on Ticket {ticketId}";
        var body = $@"
            <h2>New Comment Added to Your Ticket</h2>
            <p><strong>Ticket ID:</strong> {HttpUtility.HtmlEncode(ticketId)}</p>
            <p><strong>Comment:</strong></p>
            <p>{HttpUtility.HtmlEncode(commentContent)}</p>
            <br/>
            <p>Best regards,<br/>Helpdesk Support Team</p>
        ";

        await SendEmailAsync(recipientEmail, subject, body);
    }


    public async Task SendWelcomeEmailAsync(string email,string firstName)
    {
        var subject = "Witaj w Helpdesk System!";
        var body = $@"
        <h1>Cześć {firstName}!</h1>
        <p>Dziękujemy za rejestrację w naszym systemie Helpdesk.</p>
        <p>Możesz teraz zgłaszać problemy i śledzić ich status.</p>
        <br/>
        <p>Pozdrawiamy,<br/>Zespół Helpdesk</p>
        ";
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendLoginEmailAsync(string email)
    {
        var subject = "Nowe logowanie do Twojego konta";
        var body = $@"
        <h2>Wykryto nowe logowanie</h2>
        <p>Zalogowano się na Twoje konto ({email}) w systemie Helpdesk.</p>
        <p>Data: {DateTime.UtcNow} (UTC)</p>
        <p>Jeśli to nie Ty, skontaktuj się z administratorem.</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendTicketStatusChangedEmailAsync(string email, string firstName, string ticketId, string title, string oldStatus, string newStatus)
    {
        var subject = $"📝 Status zmieniony: {title}";

        var body = EmailTemplates.TicketStatusChanged(
            firstName: firstName,
            ticketId: Guid.Parse(ticketId),
            title: title,
            oldStatus: oldStatus,
            newStatus: newStatus
        );

        await SendEmailAsync(email, subject, body, isHtml: true);
    }
}
