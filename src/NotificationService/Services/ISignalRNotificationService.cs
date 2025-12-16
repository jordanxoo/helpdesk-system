using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using Shared.DTOs;
namespace NotificationService.Services;



public interface ISignalRNotificationService
{
    Task SendNotificationToUser(string userId, NotificationDto notification);
    Task SendNotificationToAll(NotificationDto notification);
    Task SendTicketUpdate(string userId, NotificationDto ticketData);
}