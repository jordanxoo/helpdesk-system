
namespace KnowledgeBaseService.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
