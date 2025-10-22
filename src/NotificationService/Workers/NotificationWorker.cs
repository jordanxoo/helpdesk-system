namespace NotificationService.Workers;

public class NotificationWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: Implementacja workera do konsumowania wiadomości z kolejki
        await Task.CompletedTask;
    }
}
