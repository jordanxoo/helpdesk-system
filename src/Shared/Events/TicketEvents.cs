namespace Shared.Events;

/// <summary>
/// Event published when a new ticket is created.
/// NotificationService should fetch customer details from UserService/AuthService using CustomerId.
/// </summary>
public record TicketCreatedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public Guid CustomerId { get; init; }
}

public record TicketClosedEvent : BaseEvent
{
    public Guid TicketId{get;init;}
    public Guid CustomerId {get;init;}
}

/// <summary>
/// Event published when a ticket is assigned to an agent.
/// NotificationService should fetch agent details from UserService using AgentId.
/// </summary>
public record TicketAssignedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public Guid AgentId { get; init; }
    public Guid CustomerId { get; init; }
}

/// <summary>
/// Event published when a ticket status changes.
/// NotificationService should fetch customer details from UserService using CustomerId.
/// </summary>
public record TicketStatusChangedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public Guid? AgentId { get; init; }
}

/// <summary>
/// Event published when a comment is added to a ticket.
/// NotificationService should fetch user details from UserService using UserId.
/// </summary>
public record CommentAddedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public Guid CommentId { get; init; }
    public Guid UserId { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public bool IsInternal { get; init; }
}

/// <summary>
/// Event published when a ticket priority changes.
/// </summary>
public record TicketPriorityChangedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public string OldPriority { get; init; } = string.Empty;
    public string NewPriority { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public Guid? AgentId { get; init; }
}