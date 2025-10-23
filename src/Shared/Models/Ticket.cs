namespace Shared.Models;

public class Ticket
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketCategory Category { get; set; }

    public Guid CustomerId { get; set; }
    public Guid? AssignedAgentId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public List<TicketComment> Comments { get; set; } = new();

}

public class TicketComment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public bool IsInternal { get; set; } // czy komentarz widoczny tylko dla agentow

    public Ticket? Ticket { get; set; }
}

public enum TicketStatus
{
    New,
    Open,
    InProgress,
    Pending,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TicketCategory
{
    Hardware,
    Software,
    Network,
    Security,
    Account,
    Other
}
