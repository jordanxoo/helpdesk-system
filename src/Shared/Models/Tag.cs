namespace Shared.Models;

/// <summary>
/// Tag - elastyczna etykieta do kategoryzacji ticketów (Many-to-Many)
/// </summary>
public class Tag
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Kolor wyświetlany w UI (hex, np. "#FF5733")
    /// </summary>
    public string? Color { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties - Many-to-Many z Ticket
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
