namespace NotificationService.Configuration;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    
    public int SmtpPort { get; set; } = 587;
    
    public string SenderEmail { get; set; } = "noreply@helpdesk.com";
    
    public string SenderName { get; set; } = "Helpdesk System";
    
    public string? SmtpPassword { get; set; }
}
