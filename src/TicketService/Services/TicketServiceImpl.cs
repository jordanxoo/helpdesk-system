using Shared.Constants;
using Shared.DTOs;
using Shared.Models;
using Shared.Events;
using Shared.Messaging;
using Shared.HttpClients;
using TicketService.Repositories;

namespace TicketService.Services;

public class TicketServiceImpl : ITicketService
{
    private readonly ITicketRepository _repository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IUserServiceClient _userServiceClient;
    private readonly ILogger<TicketServiceImpl> _logger;

    public TicketServiceImpl(
        ITicketRepository repository, 
        IMessagePublisher messagePublisher,
        IUserServiceClient userServiceClient,
        ILogger<TicketServiceImpl> logger)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
        _userServiceClient = userServiceClient;
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

    public async Task<TicketDto> CreateAsync(Guid userId, string userRole, CreateTicketRequest request)
    {
        _logger.LogInformation("Creating new ticket - User: {UserId}, Role: {Role}", userId, userRole);

        if (!Enum.TryParse<TicketPriority>(request.Priority,true, out var priority))
        {
            throw new ArgumentException($"Invalid priority: {request.Priority}");
        }

        if (!Enum.TryParse<TicketCategory>(request.Category,true, out var category))
        {
            throw new ArgumentException($"Invalid category: {request.Category}");
        }

        // Determine the actual customer ID based on role
        Guid actualCustomerId;
        
        if (userRole == "Customer")
        {
            // Customer creates ticket for themselves
            actualCustomerId = userId;
            
            if (request.CustomerId.HasValue && request.CustomerId.Value != userId)
            {
                throw new ArgumentException("Customers can only create tickets for themselves");
            }
        }
        else if (userRole == "Agent" || userRole == "Administrator")
        {
            // Agent/Admin must provide CustomerId
            if (!request.CustomerId.HasValue)
            {
                throw new ArgumentException("Agents and Administrators must specify CustomerId when creating tickets");
            }
            
            actualCustomerId = request.CustomerId.Value;
            _logger.LogInformation("Agent/Admin {UserId} creating ticket for customer {CustomerId}", 
                userId, actualCustomerId);
        }
        else
        {
            throw new ArgumentException($"Invalid role: {userRole}");
        }

        // Determine OrganizationId
        Guid? organizationId = null;

        if (request.OrganizationId.HasValue)
        {
            // Manual override (usually by Agent/Admin)
            organizationId = request.OrganizationId.Value;
            _logger.LogInformation("Using manually provided OrganizationId: {OrganizationId}", organizationId);
        }
        else
        {
            // Auto-fetch from UserService based on customer
            _logger.LogDebug("Fetching organization for customer {CustomerId}", actualCustomerId);
            organizationId = await _userServiceClient.GetUserOrganizationAsync(actualCustomerId);
            
            if (organizationId.HasValue)
            {
                _logger.LogDebug("Customer {CustomerId} belongs to organization {OrganizationId}", 
                    actualCustomerId, organizationId.Value);
            }
        }

        var ticket = new Ticket
        {
            CustomerId = actualCustomerId,
            Title = request.Title,
            Description = request.Description,
            Priority = priority,
            Category = category,
            Status = TicketStatus.New,
            OrganizationId = organizationId
        };

        var createdTicket = await _repository.CreateAsync(ticket);
        _logger.LogInformation("Ticket created successfully: {TicketId} for Customer: {CustomerId} with Organization: {OrganizationId}", 
            createdTicket.Id, actualCustomerId, organizationId?.ToString() ?? "none");

        await PublishTicketCreatedEventAsync(createdTicket);

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
            )).ToList()
        );
    }

    private async Task PublishTicketCreatedEventAsync(Ticket ticket)
    {
        var eventData = new TicketCreatedEvent
        {
            TicketId = ticket.Id,
            CustomerId = ticket.CustomerId,
            Timestamp = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync(eventData, RoutingKeys.TicketCreated);
        _logger.LogInformation("Published TicketCreatedEvent for ticket {TicketId}", ticket.Id);
    }

    private async Task PublishTicketAssignedEventAsync(Ticket ticket)
    {
        if (!ticket.AssignedAgentId.HasValue) return;

        var eventData = new TicketAssignedEvent
        {
            TicketId = ticket.Id,
            AgentId = ticket.AssignedAgentId.Value,
            CustomerId = ticket.CustomerId,
            Timestamp = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync(eventData, RoutingKeys.TicketAssigned);
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
            AgentId = ticket.AssignedAgentId,
            Timestamp = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync(eventData, RoutingKeys.TicketStatusChanged);
        _logger.LogInformation("Published TicketStatusChangedEvent for ticket {TicketId}", ticket.Id);
    }

    private async Task PublishCommentAddedEventAsync(Ticket ticket, TicketComment comment)
    {
        var eventData = new CommentAddedEvent
        {
            TicketId = ticket.Id,
            CommentId = comment.Id,
            UserId = comment.UserId,
            Content = comment.Content,
            CustomerId = ticket.CustomerId,
            IsInternal = comment.IsInternal,
            Timestamp = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync(eventData, RoutingKeys.CommentAdded);
        _logger.LogInformation("Published CommentAddedEvent for ticket {TicketId}", ticket.Id);
    }
}