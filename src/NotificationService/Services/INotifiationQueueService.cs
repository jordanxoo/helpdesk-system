using MassTransit.Contracts.JobService;
using Shared.Models;

namespace NotificationService.Services;


public interface INotificationQueueService
{
    Task EnqueueNotificationAsync(Guid userId, NotificationMessage message, CancellationToken ct = default);

    Task<IEnumerable<NotificationMessage>> GetPendingNotificationsAsync(Guid userId, CancellationToken ct = default);

    Task MarkAsDeliveredAsync(Guid userId, string notificationId,CancellationToken ct = default);
    
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    Task ClearAllNotificationsAsync(Guid userId, CancellationToken ct = default);

}