using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Caching;
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Models;
using TicketService.Data;

namespace TicketService.Features.Tickets.Commands.ChangePriority;

public class ChangePriorityCommandHandler : IRequestHandler<ChangePriorityCommand, TicketDto>
{
    private readonly TicketDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ChangePriorityCommandHandler> _logger;

    public ChangePriorityCommandHandler(TicketDbContext dbContext, IDistributedCache cache, ILogger<ChangePriorityCommandHandler> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<TicketDto> Handle(ChangePriorityCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Changing ticket {ticketId} priority to {newPriority}", command.TicketId, command.NewPriority);

        var ticket = await _dbContext.Tickets
            .Include(t => t.Attachments)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, ct);

        if (ticket == null)
        {
            throw new NotFoundException("Ticket", command.TicketId);
        }

        var oldPriority = ticket.Priority;
        var newPriority = Enum.Parse<TicketPriority>(command.NewPriority, ignoreCase: true);

        ticket.Priority = newPriority;
        ticket.UpdatedAt = DateTime.UtcNow;

        _dbContext.TicketAuditLogs.Add(new TicketAuditLog
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            UserId = Guid.Empty, // TODO: Add userId from HttpContext
            Action = AuditAction.PriorityChanged,
            FieldName = "Priority",
            OldValue = oldPriority.ToString(),
            NewValue = newPriority.ToString(),
            Description = $"Priority changed from {oldPriority} to {newPriority}",
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(ct);

        // Invalidate cache
        await _cache.RemoveAsync(CacheKeys.Ticket(ticket.Id), ct);
        await _cache.RemoveAsync($"ticket:{ticket.Id}:history", ct);

        _logger.LogInformation("Ticket {ticketId} priority changed from {oldPriority} to {newPriority}", ticket.Id, oldPriority, newPriority);

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
