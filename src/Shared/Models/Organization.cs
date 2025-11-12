namespace Shared.Models;

/// <summary>
/// Organizacja/Klient korporacyjny - kontener dla użytkowników firmowych
/// </summary>
public class Organization
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    /// <summary>
    /// Email kontaktowy organizacji
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Telefon kontaktowy organizacji
    /// </summary>
    public string? ContactPhone { get; set; }
    
    /// <summary>
    /// Czy organizacja jest aktywna
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// FK do przypisanego SLA
    /// </summary>
    public Guid? SlaId { get; set; }
    
    // Navigation properties
    public SLA? Sla { get; set; }
    
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
