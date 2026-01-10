
namespace KnowledgeBaseService.Domain.Entities;

public class ArticleView
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
    public DateTime ViewedAt { get; set; }

    public Article Article { get; set; } = null!;
}
