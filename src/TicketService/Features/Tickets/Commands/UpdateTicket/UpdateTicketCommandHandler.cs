using System.Formats.Asn1;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Models;
using TicketService.Data;

namespace TicketService.Features.Tickets.Commands.UpdateTicket;


public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand,TicketDto>
{
    private readonly TicketDbContext _dbContext;
    private readonly ILogger<UpdateTicketCommandHandler> _logger;

    public UpdateTicketCommandHandler(TicketDbContext dbContext, ILogger<UpdateTicketCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(UpdateTicketCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Updating Ticket {ticketId}",command.TicketId);

        var ticket = await _dbContext.Tickets
        .Include(t => t.Comments)
        .Include(t => t.Attachments)
        .FirstOrDefaultAsync(t => t.Id == command.TicketId,ct);

        if(ticket == null)
        {
            throw new NotFoundException("Ticket", command.TicketId);

        }

        if(!string.IsNullOrEmpty(command.Title))
        {
            ticket.Title = command.Title;
        }

        if(!string.IsNullOrEmpty(command.Description))
        {
            ticket.Description = command.Description;
        }

        if(!string.IsNullOrEmpty(command.Category))
        {
            ticket.Category = Enum.Parse<TicketCategory>(command.Category, ignoreCase: true);
        }

        if(!string.IsNullOrEmpty(command.Priority))
        {
            ticket.Priority = Enum.Parse<TicketPriority>(command.Priority, ignoreCase: true);    
        }

        if(command.SlaID.HasValue)
        {
            ticket.SlaId = command.SlaID.Value;
        }

        _dbContext.TicketAuditLogs.Add(new TicketAuditLog
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            UserId = Guid.Empty, // TODO POBRAC Z HTTPCONTEX JESLI POTRZEBA
            Action = AuditAction.Updated,
            Description = $"Ticket Updated",
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Ticket {ticketId} updated successfully",ticket.Id);

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
            AssignedAgentId: ticket.AssignedAgentId,
            OrganizationId: ticket.OrganizationId,
            SlaId: ticket.SlaId,
            CreatedAt: ticket.CreatedAt,
            UpdatedAt: ticket.UpdatedAt,
            ResolvedAt: ticket.ResolvedAt,
            Comments: ticket.Comments.Select(c => new CommentDto(
                Id: c.Id,
                UserId: c.UserId,
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