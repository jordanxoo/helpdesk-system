namespace KnowledgeBaseService.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Markdown
    public string? Summary { get; set; }
    
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    
    public ArticleStatus Status { get; set; }
    
    public int ViewCount { get; set; }
    public int HelpfulVotes { get; set; }
    public int NotHelpfulVotes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    // Navigation properties
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<ArticleVote> Votes { get; set; } = new List<ArticleVote>();
    public ICollection<ArticleView> Views { get; set; } = new List<ArticleView>();
    public ICollection<ArticleAttachment> Attachments { get; set; } = new List<ArticleAttachment>();
}

public enum ArticleStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}
