using MediatR;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Caching;
using Shared.DTOs;
using Shared.Events;
using Shared.Exceptions;
using Shared.Models;
using TicketService.Data;

namespace TicketService.Features.Tickets.Commands.ChangeStatus;

public class ChangeStatusCommandHandler : IRequestHandler<ChangeStatusCommand,TicketDto>
{
    private readonly TicketDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ChangeStatusCommandHandler> _logger;


    public ChangeStatusCommandHandler(TicketDbContext dbContext, IPublishEndpoint publishEndpoint, IDistributedCache cache, ILogger<ChangeStatusCommandHandler> logger)
    {
        _logger = logger;
        _dbContext  = dbContext;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }


    public async Task<TicketDto> Handle(ChangeStatusCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Changing ticket { ticketId} status to {newStatus}",command.TicketId,command.NewStatus);

        var ticket = await _dbContext.Tickets
        .Include(t => t.Attachments)
        .Include(t => t.Comments)
        .FirstOrDefaultAsync(t => t.Id == command.TicketId, ct);

        if(ticket == null)
        {
            throw new NotFoundException("Ticket", command.TicketId);
        }

        var oldStatus = ticket.Status;
        var newStatus = Enum.Parse<TicketStatus>(command.NewStatus,ignoreCase: true);

        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        if(newStatus == TicketStatus.Resolved || newStatus == TicketStatus.Closed)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }

        _dbContext.TicketAuditLogs.Add(new TicketAuditLog
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            UserId = Guid.Empty, //TODO dodac yserId z httpcontext
            Action = AuditAction.StatusChanged,
            Description = $"Status changed from {oldStatus} to {newStatus}",
            CreatedAt = DateTime.UtcNow
        });

        if(newStatus == TicketStatus.Closed)
        {
            await _publishEndpoint.Publish(new TicketClosedEvent
            {
                TicketId = ticket.Id,
                CustomerId = ticket.CustomerId,
                Timestamp = DateTime.UtcNow
            },ct);
        }

        await _dbContext.SaveChangesAsync(ct);

        // Invalidate cache
        await _cache.RemoveAsync(CacheKeys.Ticket(ticket.Id), ct);
        await _cache.RemoveAsync($"ticket:{ticket.Id}:history", ct);

        _logger.LogInformation("Ticket {ticketId} status changed from {oldStatus} to {newStatus}",ticket.Id,oldStatus,newStatus);
    
        return MapToDto(ticket);
    }
    private static TicketDto MapToDto(Ticket ticket)
    {
        return new TicketDto(
            Id: ticket.Id,
            Title: ticket.Title,
            Description: ticket.Description,
            Status: ticket.Status.ToString(),
            Priority: ticket.Priority.ToString(),
            Category: ticket.Category.ToString(),
            CustomerId: ticket.CustomerId,
            CustomerName: null,
            AssignedAgentId: ticket.AssignedAgentId,
            AssignedAgentName: null,
            OrganizationId: ticket.OrganizationId,
            SlaId: ticket.SlaId,
            CreatedAt: ticket.CreatedAt,
            UpdatedAt: ticket.UpdatedAt,
            ResolvedAt: ticket.ResolvedAt,
            Comments: ticket.Comments.Select(c => new CommentDto(
                Id: c.Id,
                UserId: c.UserId,
                UserName: null,
                UserRole: null,
                Content: c.Content,
                CreatedAt: c.CreatedAt,
                IsInternal: c.IsInternal
            )).ToList(),
            Attachment: ticket.Attachments.Select(a => new TicketAttachmentDto(
                id: a.Id,
                FileName: a.FileName,
                ContentType: a.ContentType,
                FileSizeBytes: a.FileSizeBytes,
                DownloadUrl: a.StoragePath,
                UploadedAt: a.UploadedAt
            )).ToList()
        );
    }
}