using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Events;
using Shared.Exceptions;
using Shared.HttpClients;
using Shared.Models;
using TicketService.Data;



namespace TicketService.Features.Tickets.Commands.AssignToAgent;


public class AssignToAgentCommandHandler : IRequestHandler<AssignToAgentCommand,TicketDto>
{
    private readonly TicketDbContext _dbContext;
    private readonly ILogger<AssignToAgentCommandHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUserServiceClient _userServiceClient;

    public AssignToAgentCommandHandler(TicketDbContext dbContext, ILogger<AssignToAgentCommandHandler> logger
    ,IPublishEndpoint publishEndpoint, IUserServiceClient userServiceClient)
    {
        _dbContext = dbContext;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _userServiceClient = userServiceClient;
    }

    public async Task<TicketDto> Handle(AssignToAgentCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Assigning ticket {ticketId} to agent {agentId}",command.TicketId,command.AgentId);

        var ticket = await _dbContext.Tickets
        .Include( t => t.Comments)
        .Include( t => t.Attachments)
        .FirstOrDefaultAsync(t => t.Id == command.TicketId,ct);


        if(ticket == null)
        {
            throw new NotFoundException("Ticket", command.TicketId);
        }

        try
        {
            var agentExists = await _userServiceClient.GetUserAsync(command.AgentId);

            if(agentExists == null)
            {
                throw new NotFoundException("Agent", command.AgentId);
            }
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to verify if agent exsists");
        }

        var previousAgentId = ticket.AssignedAgentId;
        ticket.AssignedAgentId = command.AgentId;
        ticket.UpdatedAt = DateTime.UtcNow;

        if(ticket.Status == TicketStatus.New)
        {
            ticket.Status = TicketStatus.Assigned;
        }

        _dbContext.TicketAuditLogs.Add(new TicketAuditLog
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            UserId = command.AgentId,
            Action = AuditAction.Assigned,
            Description = previousAgentId.HasValue ? 
            $"Ticket reassigned from {previousAgentId} to {command.AgentId}" :
            $"Ticket assigned to agent {command.AgentId}", 
            CreatedAt = DateTime.UtcNow
        });

        await _publishEndpoint.Publish(new TicketAssignedEvent
        {
            TicketId = ticket.Id,
            AgentId = command.AgentId,
            CustomerId = ticket.CustomerId,
            Timestamp = DateTime.UtcNow
        },ct);

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Ticket {ticketId} assigned to agent {agentId}",ticket.Id,command.AgentId);

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