namespace Shared.Models;

/// <summary>
/// Audit log - pełny ślad audytowy zmian w tickecie
/// </summary>
public class TicketAuditLog
{
    public Guid Id { get; set; }
    
    public Guid TicketId { get; set; }
    
    /// <summary>
    /// Użytkownik który dokonał zmiany
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Typ akcji/zmiany
    /// </summary>
    public AuditAction Action { get; set; }
    
    /// <summary>
    /// Nazwa zmienionego pola (np. "Status", "Priority", "AssignedAgentId")
    /// </summary>
    public string? FieldName { get; set; }
    
    /// <summary>
    /// Poprzednia wartość (JSON)
    /// </summary>
    public string? OldValue { get; set; }
    
    /// <summary>
    /// Nowa wartość (JSON)
    /// </summary>
    public string? NewValue { get; set; }
    
    /// <summary>
    /// Dodatkowy opis/kontekst zmiany
    /// </summary>
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Ticket? Ticket { get; set; }
}

/// <summary>
/// Typ akcji w audit logu
/// </summary>
public enum AuditAction
{
    Created,
    Updated,
    StatusChanged,
    PriorityChanged,
    Assigned,
    Unassigned,
    CommentAdded,
    AttachmentAdded,
    AttachmentRemoved,
    Closed,
    Reopened
}
