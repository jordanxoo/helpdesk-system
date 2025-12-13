using AuthService.Services;
namespace AuthService.Workers;


public class SessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public SessionCleanupService(IServiceProvider serviceProvider, ILogger<SessionCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SeassionCleanupService started");
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting session cleanup...");
                
                using(var scope = _serviceProvider.CreateScope())
                {
                    var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
                    await sessionService.CleanupExpiredSessionsAsync(stoppingToken);
                }
                _logger.LogInformation("Session cleanup completed successfully");
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup phase");
            }
            await Task.Delay(_interval,stoppingToken);
        }
        _logger.LogInformation("SessionCleanup stopped");
    }

}