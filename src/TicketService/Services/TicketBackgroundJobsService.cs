using Microsoft.EntityFrameworkCore;
using TicketService.Data;
using Shared.Models;
using MassTransit;
using Shared.Events;
using Hangfire.Logging;



namespace TicketService.Services;

public class TicketBackgroundJobsService : ITicketBackgroundJobsService
{
    private readonly TicketDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<TicketBackgroundJobsService> _logger;


    public TicketBackgroundJobsService(TicketDbContext dbContext, IPublishEndpoint publishEndpoint, ILogger<TicketBackgroundJobsService> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<int> CheckOverdueSlaTicketsAsync()
    {
        _logger.LogInformation("Starting SLA overdue tickets check");
        var slaThreshold = DateTime.UtcNow.AddHours(-48);

        var overDueTickets = await _dbContext.Tickets.Where
        (
            t => t.Status != TicketStatus.Resolved &&
            t.Status != TicketStatus.Closed
            && t.CreatedAt < slaThreshold
            && t.Priority != TicketPriority.Critical
        ).ToListAsync();

        foreach(var ticket in overDueTickets)
        {
            var oldPriority = ticket.Priority;
            ticket.Priority = TicketPriority.Critical;
            ticket.UpdatedAt = DateTime.UtcNow;

            _dbContext.TicketAuditLogs.Add(new TicketAuditLog
            {
                TicketId = ticket.Id,
                Action = AuditAction.Updated,
                Description = $"SLA breached - Priority escalated from {oldPriority} to Critical",
                
            });

            await _publishEndpoint.Publish(new TicketSlaBreachedEvent
            {
                TicketId = ticket.Id,
                Title = ticket.Title,
                CustomerId = ticket.CustomerId,
                AgentId = ticket.AssignedAgentId,
                CreatedAt = ticket.CreatedAt,
                HoursOverdue = (int)(DateTime.UtcNow - ticket.CreatedAt).TotalHours,
                Timestamp = DateTime.UtcNow
            });
            _logger.LogWarning("Ticket {TicketId} breached SLA - promoted to Critical",ticket.Id);
        }

            if(overDueTickets.Count > 0)
            {
                await _dbContext.SaveChangesAsync();
            }

        
        _logger.LogInformation("SLA check - completed - {Count} tickets overdue ", overDueTickets.Count);
            return overDueTickets.Count;
    }

    public async Task<int> AutoCloseResolvedTicketsAsync()
    {
        _logger.LogInformation("Starting auto-close for resolved tickets");
        var autoCloseThreshold = DateTime.UtcNow.AddDays(-7);

        var ticketsToClose = await _dbContext.Tickets.
        Where(t => t.Status == TicketStatus.Resolved 
        && t.UpdatedAt < autoCloseThreshold).ToListAsync();
    
        foreach( var ticket in ticketsToClose)
        {
            var oldStatus = ticket.Status;
            ticket.Status = TicketStatus.Closed; 
            ticket.UpdatedAt = DateTime.UtcNow;

            _dbContext.TicketComments.Add(new TicketComment
            {
                TicketId = ticket.Id,
                UserId = Guid.Empty,
                Content = "Ticket automatically closed after 7 days in Resolved status.",
                IsInternal = true,
                CreatedAt = DateTime.UtcNow
            });

            _dbContext.TicketAuditLogs.Add(new TicketAuditLog
            {
                TicketId = ticket.Id,
                Action = AuditAction.StatusChanged,
                Description = $"Auto-closed after 7 days in Resolved status",
                CreatedAt = DateTime.UtcNow
            });

            await _publishEndpoint.Publish(new TicketStatusChangedEvent
            {
                TicketId = ticket.Id,
                OldStatus = oldStatus.ToString(),
                NewStatus = TicketStatus.Closed.ToString(),
                CustomerId = ticket.CustomerId,
                AgentId = ticket.AssignedAgentId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Auto-closed ticket {TicketId}", ticket.Id);
        }

        if(ticketsToClose.Count > 0)
        {
            await _dbContext.SaveChangesAsync();
        }
        _logger.LogInformation("Auto-close completed - {Count} tickets closed",ticketsToClose.Count);
        return ticketsToClose.Count;
    }

    public async Task<int> SendTicketRemindersAsync()
    {
        _logger.LogInformation("Starting ticket reminders check");
        var reminderThreshold = DateTime.UtcNow.AddDays(-1);

        var ticketsNeedingReminder = await _dbContext.Tickets.Include(t => t.Comments).Where(t => t.Status == TicketStatus.Open &&
            t.CreatedAt < reminderThreshold && !t.Comments.Any()).ToListAsync();

        foreach(var ticket in ticketsNeedingReminder)
        {
            await _publishEndpoint.Publish(new TicketReminderEvent
            {
                TicketId = ticket.Id,
                Title = ticket.Title,
                CustomerId = ticket.CustomerId,
                AgentId = ticket.AssignedAgentId,
                HoursSinceCreated = (int)(DateTime.UtcNow - ticket.CreatedAt).TotalHours,
                Timestamp = DateTime.UtcNow
            });

            // Update ticket to mark that reminder was sent (needed for MassTransit Outbox to work)
            ticket.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Sent reminder for ticket {TicketId}", ticket.Id);
        }

        // Save changes to commit the outbox messages
        if(ticketsNeedingReminder.Count > 0)
        {
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("Reminders sent - {Count} tickets", ticketsNeedingReminder.Count);
        return ticketsNeedingReminder.Count;
    }
}