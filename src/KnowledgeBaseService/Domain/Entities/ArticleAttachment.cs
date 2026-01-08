
namespace KnowledgeBaseService.Domain.Entities;

public class ArticleAttachment
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string? S3Url { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public Article Article { get; set; } = null!;
}
