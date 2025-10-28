namespace NotificationService.Services;

/// <summary>
/// SMS service implementation.
/// For production, integrate with AWS SNS, Twilio, or similar service.
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            _logger.LogInformation("Sending SMS to {PhoneNumber}: {Message}", phoneNumber, message);
            
            // For development - just log instead of actually sending
            // In production, integrate with AWS SNS, Twilio, etc.
            _logger.LogWarning("SMS service not fully implemented. SMS would be sent to {PhoneNumber}", phoneNumber);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            throw;
        }
    }

    public async Task SendTicketCreatedSmsAsync(string phoneNumber, string ticketId)
    {
        var message = $"Your support ticket {ticketId} has been created. We'll get back to you soon.";
        await SendSmsAsync(phoneNumber, message);
    }
}
