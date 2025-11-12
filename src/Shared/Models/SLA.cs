namespace Shared.Models;

/// <summary>
/// Service Level Agreement - umowa serwisowa definiująca czasy reakcji i rozwiązania
/// </summary>
public class SLA
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    /// <summary>
    /// Czas odpowiedzi dla ticketów Critical (w minutach)
    /// </summary>
    public int ResponseTimeCritical { get; set; } = 60; // 1 godzina
    
    /// <summary>
    /// Czas odpowiedzi dla ticketów High (w minutach)
    /// </summary>
    public int ResponseTimeHigh { get; set; } = 240; // 4 godziny
    
    /// <summary>
    /// Czas odpowiedzi dla ticketów Medium (w minutach)
    /// </summary>
    public int ResponseTimeMedium { get; set; } = 480; // 8 godzin
    
    /// <summary>
    /// Czas odpowiedzi dla ticketów Low (w minutach)
    /// </summary>
    public int ResponseTimeLow { get; set; } = 1440; // 24 godziny
    
    /// <summary>
    /// Czas rozwiązania dla ticketów Critical (w minutach)
    /// </summary>
    public int ResolutionTimeCritical { get; set; } = 240; // 4 godziny
    
    /// <summary>
    /// Czas rozwiązania dla ticketów High (w minutach)
    /// </summary>
    public int ResolutionTimeHigh { get; set; } = 480; // 8 godzin
    
    /// <summary>
    /// Czas rozwiązania dla ticketów Medium (w minutach)
    /// </summary>
    public int ResolutionTimeMedium { get; set; } = 1440; // 1 dzień
    
    /// <summary>
    /// Czas rozwiązania dla ticketów Low (w minutach)
    /// </summary>
    public int ResolutionTimeLow { get; set; } = 4320; // 3 dni
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
