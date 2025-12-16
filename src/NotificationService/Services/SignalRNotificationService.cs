using Microsoft.AspNetCore.SignalR;
using Shared.Models;
using NotificationService.Hubs;
using Shared.DTOs;
namespace NotificationService.Services;


public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly ILogger<SignalRNotificationService> _logger;
    private readonly IHubContext<NotificationHub> _context;

    public SignalRNotificationService(ILogger<SignalRNotificationService> logger, IHubContext<NotificationHub> context)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SendNotificationToUser(string userId, NotificationDto notification)
    {
        try
        {
            await _context.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification",notification);
            _logger.LogInformation("Sent notification to user {userId}",userId);
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {userId}",userId);
        }
    }

    public async Task SendNotificationToAll(NotificationDto notification)
    {
        try
        {
            await _context.Clients.All.SendAsync("ReceiveNotification",notification);
        }catch(Exception ex)
        {
            _logger.LogError(ex,"Error sending notification to all users");
        }
    }
    public async Task SendTicketUpdate(string userId,NotificationDto ticketData)
    {
        try
        {
            await _context.Clients.Group($"user_{userId}")
            .SendAsync("TicketUpdated",ticketData);
            _logger.LogInformation($"Sent ticket update to user {userId}",userId);
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error sending ticket update to user {userId}",userId);
        }
    }
}