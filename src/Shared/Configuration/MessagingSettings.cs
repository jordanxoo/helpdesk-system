namespace Shared.Configuration;

public class MessagingSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "helpdesk-events";
    public bool AutoCreateInfrastructure { get; set; } = true;
    public int ConnectionTimeout { get; set; } = 30;
    public bool PersistentMessages { get; set; } = true;
}
