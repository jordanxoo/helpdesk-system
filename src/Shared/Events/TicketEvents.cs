namespace Shared.Events;

public record TicketCreatedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public Guid CustomerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
}

public record TicketAssignedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public Guid AgentId { get; init; }
    public Guid CustomerId { get; init; }
}

public record TicketStatusChangedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public Guid? AgentId { get; init; }
}

public record CommentAddedEvent : BaseEvent
{
    public Guid TicketId { get; init; }
    public Guid CommentId { get; init; }
    public Guid UserId { get; init; }
    public Guid CustomerId { get; init; }
    public bool IsInternal { get; init; }
}