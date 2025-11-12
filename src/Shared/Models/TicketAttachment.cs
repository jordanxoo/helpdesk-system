namespace Shared.Models;

/// <summary>
/// Załącznik do ticketa - metadata (fizyczny plik w storage zewnętrznym)
/// </summary>
public class TicketAttachment
{
    public Guid Id { get; set; }
    
    public Guid TicketId { get; set; }
    
    /// <summary>
    /// Użytkownik który dodał załącznik
    /// </summary>
    public Guid UploadedById { get; set; }
    
    /// <summary>
    /// Oryginalna nazwa pliku
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Typ MIME pliku (np. "image/png", "application/pdf")
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Rozmiar pliku w bajtach
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// Ścieżka/klucz w storage (S3 key, file path, etc.)
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;
    
    /// <summary>
    /// URL do pobrania (może być pre-signed S3 URL)
    /// </summary>
    public string? DownloadUrl { get; set; }
    
    public DateTime UploadedAt { get; set; }
    
    // Navigation properties
    public Ticket? Ticket { get; set; }
}
