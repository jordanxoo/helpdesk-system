
namespace KnowledgeBaseService.Domain.Entities;

public class ArticleVote
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
    public bool IsHelpful { get; set; }
    public string? Feedback { get; set; }
    public DateTime CreatedAt { get; set; }

    public Article Article { get; set; } = null!;
}
