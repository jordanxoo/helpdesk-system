using Shared.DTOs;
using Shared.Models;
using Shared.Events;
using Shared.Messaging;
using TicketService.Repositories;

namespace TicketService.Services;

public class TicketServiceImpl : ITicketService
{
    private readonly ITicketRepository _repository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<TicketServiceImpl> _logger;

    public TicketServiceImpl(
        ITicketRepository repository, 
        IMessagePublisher messagePublisher, 
        ILogger<TicketServiceImpl> logger)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<TicketDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching ticket with id: {TicketId}", id);
        var ticket = await _repository.GetByIdAsync(id, includeComments: true);
        return ticket != null ? MapToDto(ticket) : null;
    }

    public async Task<TicketListResponse> GetAllAsync(int page, int pageSize)
    {
        _logger.LogInformation("Fetching all tickets - Page: {Page}, PageSize: {PageSize}", page, pageSize);
        var tickets = await _repository.GetAllAsync(page, pageSize);
        var totalCount = await _repository.GetTotalCountAsync(null, null, null, null, null, null);

        return new TicketListResponse(
            Tickets: tickets.Select(MapToDto).ToList(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize
        );
    }

    public async Task<TicketListResponse> SearchAsync(TicketFilterRequest filter)
    {
        _logger.LogInformation("Searching tickets with filter: {@Filter}", filter);

        var tickets = await _repository.SearchAsync(
            filter.SearchTerm,
            filter.Status,
            filter.Priority,
            filter.Category,
            filter.CustomerId,
            filter.AssignedAgentId,
            filter.Page,
            filter.PageSize
        );

        var totalCount = await _repository.GetTotalCountAsync(
            searchTerm: filter.SearchTerm,
            status: filter.Status,
            priority: filter.Priority,
            category: filter.Category,
            customerId: filter.CustomerId,
            assignedAgentId: filter.AssignedAgentId
        );

        return new TicketListResponse(
            Tickets: tickets.Select(MapToDto).ToList(),
            TotalCount: totalCount,
            Page: filter.Page,
            PageSize: filter.PageSize
        );
    }

    public async Task<TicketListResponse> GetMyTicketsAsync(Guid customerId, int page, int pageSize)
    {
        _logger.LogInformation("Fetching tickets for customer: {CustomerId}", customerId);

        var tickets = await _repository.GetByCustomerIdAsync(customerId, page, pageSize);
        var totalCount = await _repository.GetTotalCountAsync(null, null, null, null, customerId, null);

        return new TicketListResponse(
            Tickets: tickets.Select(MapToDto).ToList(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize
        );
    }

    public async Task<TicketListResponse> GetAssignedTicketsAsync(Guid agentId, int page, int pageSize)
    {
        _logger.LogInformation("Fetching tickets assigned to agent: {AgentId}", agentId);

        var tickets = await _repository.GetByAgentIdAsync(agentId, page, pageSize);
        var totalCount = await _repository.GetTotalCountAsync(null, null, null, null, null, agentId);

        return new TicketListResponse(
            Tickets: tickets.Select(MapToDto).ToList(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize
        );
    }

    public async Task<TicketDto> CreateAsync(Guid customerId, string customerEmail, CreateTicketRequest request)
    {
        _logger.LogInformation("Creating new ticket for customer: {CustomerId}", customerId);

        if (!Enum.TryParse<TicketPriority>(request.Priority, out var priority))
        {
            throw new ArgumentException($"Invalid priority: {request.Priority}");
        }

        if (!Enum.TryParse<TicketCategory>(request.Category, out var category))
        {
            throw new ArgumentException($"Invalid category: {request.Category}");
        }

        var ticket = new Ticket
        {
            CustomerId = customerId,
            Title = request.Title,
            Description = request.Description,
            Priority = priority,
            Category = category,
            Status = TicketStatus.New
        };

        var createdTicket = await _repository.CreateAsync(ticket);
        _logger.LogInformation("Ticket created successfully: {TicketId}", createdTicket.Id);

        await PublishTicketCreatedEventAsync(createdTicket, customerEmail);

        return MapToDto(createdTicket);
    }

    public async Task<TicketDto> UpdateAsync(Guid id, UpdateTicketRequest request)
    {
        _logger.LogInformation("Updating ticket: {TicketId}", id);

        var ticket = await _repository.GetByIdAsync(id, includeComments: false);
        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with id {id} not found");
        }

        var oldStatus = ticket.Status.ToString();

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            ticket.Title = request.Title;
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            ticket.Description = request.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && 
            Enum.TryParse<TicketStatus>(request.Status, out var status))
        {
            ticket.Status = status;
        }

        if (!string.IsNullOrWhiteSpace(request.Priority) && 
            Enum.TryParse<TicketPriority>(request.Priority, out var priority))
        {
            ticket.Priority = priority;
        }

        if (request.AssignedAgentId.HasValue)
        {
            ticket.AssignedAgentId = request.AssignedAgentId.Value;
        }

        var updatedTicket = await _repository.UpdateAsync(ticket);
        _logger.LogInformation("Ticket updated successfully: {TicketId}", id);

        if (oldStatus != updatedTicket.Status.ToString())
        {
            await PublishTicketStatusChangedEventAsync(updatedTicket, oldStatus);
        }

        return MapToDto(updatedTicket);
    }

    public async Task<TicketDto> AssignToAgentAsync(Guid ticketId, Guid agentId)
    {
        _logger.LogInformation("Assigning ticket {TicketId} to agent {AgentId}", ticketId, agentId);

        var ticket = await _repository.GetByIdAsync(ticketId, includeComments: false);
        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with id {ticketId} not found.");
        }

        ticket.AssignedAgentId = agentId;
        ticket.Status = TicketStatus.Open;

        var updatedTicket = await _repository.UpdateAsync(ticket);
        _logger.LogInformation("Ticket assigned successfully");

        await PublishTicketAssignedEventAsync(updatedTicket);

        return MapToDto(updatedTicket);
    }

    public async Task<TicketDto> ChangeStatusAsync(Guid ticketId, string newStatus)
    {
        _logger.LogInformation("Changing ticket {TicketId} status to {NewStatus}", ticketId, newStatus);

        if (!Enum.TryParse<TicketStatus>(newStatus, out var status))
        {
            throw new ArgumentException($"Invalid status: {newStatus}");
        }

        var ticket = await _repository.GetByIdAsync(ticketId, includeComments: false);
        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with id {ticketId} not found");
        }

        var oldStatus = ticket.Status.ToString();
        ticket.Status = status;

        var updatedTicket = await _repository.UpdateAsync(ticket);
        _logger.LogInformation("Ticket status changed successfully");

        await PublishTicketStatusChangedEventAsync(updatedTicket, oldStatus);

        return MapToDto(updatedTicket);
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting ticket: {TicketId}", id);

        var ticket = await _repository.GetByIdAsync(id, includeComments: false);
        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with id {id} not found");
        }

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Ticket deleted successfully: {TicketId}", id);
    }

    public async Task<TicketDto> AddCommentAsync(Guid ticketId, Guid userId, AddCommentRequest request)
    {
        _logger.LogInformation("Adding comment to ticket {TicketId} by user {UserId}", ticketId, userId);

        var ticket = await _repository.GetByIdAsync(ticketId, includeComments: false);
        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with id {ticketId} not found");
        }

        var comment = new TicketComment
        {
            TicketId = ticketId,
            UserId = userId,
            Content = request.Content,
            IsInternal = request.IsInternal
        };

        await _repository.AddCommentAsync(comment);
        _logger.LogInformation("Comment added successfully");

        await PublishCommentAddedEventAsync(ticket, comment);

        var updatedTicket = await _repository.GetByIdAsync(ticketId, includeComments: true);
        return MapToDto(updatedTicket!);
    }

    // Private helper methods

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
            CreatedAt: ticket.CreatedAt,
            UpdatedAt: ticket.UpdatedAt,
            ResolvedAt: ticket.ResolvedAt,
            Comments: ticket.Comments.Select(c => new CommentDto(
                Id: c.Id,
                UserId: c.UserId,
                Content: c.Content,
                CreatedAt: c.CreatedAt,
                IsInternal: c.IsInternal
            )).ToList()
        );
    }

    private async Task PublishTicketCreatedEventAsync(Ticket ticket, string customerEmail)
    {
        var eventData = new TicketCreatedEvent
        {
            TicketId = ticket.Id,
            CustomerId = ticket.CustomerId,
            CustomerEmail = customerEmail,
            CustomerPhone = null, // Could be added to user profile later
            Title = ticket.Title,
            Priority = ticket.Priority.ToString(),
            Category = ticket.Category.ToString(),
            Timestamp = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync(eventData, "ticket-created");
        _logger.LogInformation("Published TicketCreatedEvent for ticket {TicketId}", ticket.Id);
    }

    private async Task PublishTicketAssignedEventAsync(Ticket ticket)
    {
        if (!ticket.AssignedAgentId.HasValue) return;

        var eventData = new TicketAssignedEvent
        {
            TicketId = ticket.Id,
            Title = ticket.Title,
            AgentId = ticket.AssignedAgentId.Value,
            AgentEmail = $"agent-{ticket.AssignedAgentId.Value}@helpdesk.com", // TODO: Get from UserService
            CustomerId = ticket.CustomerId,
            Timestamp = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync(eventData, "ticket-assigned");
        _logger.LogInformation("Published TicketAssignedEvent for ticket {TicketId}", ticket.Id);
    }

    private async Task PublishTicketStatusChangedEventAsync(Ticket ticket, string oldStatus)
    {
        var eventData = new TicketStatusChangedEvent
        {
            TicketId = ticket.Id,
            OldStatus = oldStatus,
            NewStatus = ticket.Status.ToString(),
            CustomerId = ticket.CustomerId,
            CustomerEmail = $"customer-{ticket.CustomerId}@helpdesk.com", // TODO: Get from UserService or cache
            AgentId = ticket.AssignedAgentId,
            Timestamp = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync(eventData, "ticket-status-changed");
        _logger.LogInformation("Published TicketStatusChangedEvent for ticket {TicketId}", ticket.Id);
    }

    private async Task PublishCommentAddedEventAsync(Ticket ticket, TicketComment comment)
    {
        var eventData = new CommentAddedEvent
        {
            TicketId = ticket.Id,
            CommentId = comment.Id,
            UserId = comment.UserId,
            UserName = "User", // TODO: Get from context or UserService
            Content = comment.Content,
            CustomerId = ticket.CustomerId,
            RecipientEmail = $"customer-{ticket.CustomerId}@helpdesk.com", // TODO: Get from UserService
            IsInternal = comment.IsInternal,
            Timestamp = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync(eventData, "comment-added");
        _logger.LogInformation("Published CommentAddedEvent for ticket {TicketId}", ticket.Id);
    }
}