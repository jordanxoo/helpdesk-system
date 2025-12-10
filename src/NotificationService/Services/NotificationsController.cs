using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Services;
using Shared.Models;



///<summary>
/// API do zarzadzania powiadomieniami uzytkownikow 
/// fronted moze pobrac powiadomienia i oznaczac je jako odebrane
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class NotificationsController: ControllerBase
{
    private readonly INotificationQueueService _queueService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationQueueService queueService, ILogger<NotificationsController> logger)
    {
        _queueService = queueService;
        _logger = logger;
    }

    [HttpGet("{userID:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<NotificationMessage>>> GetPendingNotifications(Guid userId)
    {
        try
        {
            var notifications = await _queueService.GetPendingNotificationsAsync(userId);
            return Ok(notifications);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,"Error getting notifications for user {userId}",userId);
            return StatusCode(500, "Error retrieving notifications");
        }
    }
    [HttpGet("{userId:guid}/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> GetUnreadCount(Guid userId)
    {
        try
        {
            var count = await _queueService.GetUnreadCountAsync(userId);
            return Ok(count);
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {userId}",userId);
            return StatusCode(500,"Error retrieving unread count");
        }
    }
    [HttpDelete("{userId:guid}/{notificationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAsRead(Guid userId, string notificationId)
    {
        try
        {
            await _queueService.MarkAsDeliveredAsync(userId,notificationId);
            return NoContent();
        }catch(Exception ex)
        {
            _logger.LogError(ex,"Error marking notification {notificationId} as read for user {userId}",
            notificationId, userId);
            return StatusCode(500,"Error marking notification as read");
        }
    }

    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearAllNotifications(Guid userId)
    {
        try
        {
            await _queueService.ClearAllNotificationsAsync(userId);
            return NoContent();
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error clearing notifications for user {userId}", userId);
            return StatusCode(500,"Error clearing notifications");
        }
    }

    [HttpPost("{userId:guid}/test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTestNotification(Guid userId)
    {
        try
        {
            var notification = NotificationMessage.Create(
                type: "Test",
                title: "Test notification",
                body: "This is a test notification from the API",
                metadata: new Dictionary<string, string>
                {
                    ["createdVia"] = "API",
                    ["timestamp"] = DateTime.UtcNow.ToString()
                }
            );

            await _queueService.EnqueueNotificationAsync(userId,notification);

            return Ok(new
            {
                Message = "Test notification created",
                notificationId = notification.id,
                userId
            });
        }catch(Exception ex)
        {
            _logger.LogError(ex,"Error creating test notification for user {userId}",userId);
            return StatusCode(500, "Error creating test notification");
        }
    }
}