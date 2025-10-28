namespace Shared.Events;

/// <summary>
/// Event published when a new ticket is created.
/// </summary>
public record TicketCreatedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string? CustomerPhone { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}

/// <summary>
/// Event published when a ticket is assigned to an agent.
/// </summary>
public record TicketAssignedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid AgentId { get; init; }
    public string AgentEmail { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
}

/// <summary>
/// Event published when a ticket status changes.
/// </summary>
public record TicketStatusChangedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public Guid? AgentId { get; init; }
}

/// <summary>
/// Event published when a comment is added to a ticket.
/// </summary>
public record CommentAddedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public Guid CommentId { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public string RecipientEmail { get; init; } = string.Empty;
    public bool IsInternal { get; init; }
}