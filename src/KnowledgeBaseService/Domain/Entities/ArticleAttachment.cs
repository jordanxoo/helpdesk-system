
namespace KnowledgeBaseService.Domain.Entities;

public class ArticleAttachment
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
    public DateTime UploadedAt { get; set; }

    public Article Article { get; set; } = null!;
}
