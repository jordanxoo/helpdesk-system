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
    
    /// <summary>
    /// FK do organizacji (opcjonalne - dla ticketów korporacyjnych)
    /// </summary>
    public Guid? OrganizationId { get; set; }
    
    /// <summary>
    /// FK do SLA - "zamrożone" SLA w momencie utworzenia ticketa
    /// </summary>
    public Guid? SlaId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public Organization? Organization { get; set; }
    public SLA? Sla { get; set; }
    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    public ICollection<TicketAuditLog> AuditLogs { get; set; } = new List<TicketAuditLog>();
    
    /// <summary>
    /// Oblicza deadline dla pierwszej odpowiedzi na podstawie SLA
    /// </summary>
    public DateTime? GetResponseDeadline()
    {
        if (Sla == null) return null;
        
        var responseMinutes = Priority switch
        {
            TicketPriority.Critical => Sla.ResponseTimeCritical,
            TicketPriority.High => Sla.ResponseTimeHigh,
            TicketPriority.Medium => Sla.ResponseTimeMedium,
            TicketPriority.Low => Sla.ResponseTimeLow,
            _ => Sla.ResponseTimeMedium
        };
        
        return CreatedAt.AddMinutes(responseMinutes);
    }
    
    /// <summary>
    /// Oblicza deadline dla rozwiązania ticketa na podstawie SLA
    /// </summary>
    public DateTime? GetResolutionDeadline()
    {
        if (Sla == null) return null;
        
        var resolutionMinutes = Priority switch
        {
            TicketPriority.Critical => Sla.ResolutionTimeCritical,
            TicketPriority.High => Sla.ResolutionTimeHigh,
            TicketPriority.Medium => Sla.ResolutionTimeMedium,
            TicketPriority.Low => Sla.ResolutionTimeLow,
            _ => Sla.ResolutionTimeMedium
        };
        
        return CreatedAt.AddMinutes(resolutionMinutes);
    }

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
    Closed,
    Assigned
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
