using System.Net;
using System.Net.Mail;
using System.Web;
using Microsoft.Extensions.Options;
using NotificationService.Configuration;

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
            _logger.LogInformation("Sending email to {To}, Subject: {Subject}", to, subject);

            // For development - just log instead of actually sending
            if (string.IsNullOrEmpty(_settings.SmtpPassword))
            {
                _logger.LogWarning("SMTP not configured. Email would be sent to {To}: {Subject}", to, subject);
                _logger.LogDebug("Email body: {Body}", body);
                return;
            }

            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SmtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendTicketCreatedNotificationAsync(string customerEmail, string ticketId, string title)
    {
        var subject = $"Ticket Created: {title}";
        var body = $@"
            <h2>Your Support Ticket Has Been Created</h2>
            <p>Thank you for contacting our support team.</p>
            <p><strong>Ticket ID:</strong> {HttpUtility.HtmlEncode(ticketId)}</p>
            <p><strong>Title:</strong> {HttpUtility.HtmlEncode(title)}</p>
            <p>We will review your request and get back to you as soon as possible.</p>
            <p>You can track the status of your ticket in your dashboard.</p>
            <br/>
            <p>Best regards,<br/>Helpdesk Support Team</p>
        ";

        await SendEmailAsync(customerEmail, subject, body);
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
}
