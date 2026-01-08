
namespace KnowledgeBaseService.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; 
    public string? ContentHtml { get; set; }
    
    public Guid CategoryId { get; set; }
    public Guid AuthorId { get; set; }
    
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public bool IsFeatured { get; set; }
    
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    
    public string? MetaDescription { get; set; }
    
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }


    public Category Category { get; set; } = null!;
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<ArticleVote> Votes { get; set; } = new List<ArticleVote>();
    public ICollection<ArticleView> Views { get; set; } = new List<ArticleView>();
    public ICollection<ArticleAttachment> Attachments { get; set; } = new List<ArticleAttachment>();

    public double HelpfulnessScore => 
        (HelpfulCount + NotHelpfulCount) == 0 
            ? 0 
            : (double)HelpfulCount / (HelpfulCount + NotHelpfulCount) * 100;
}

public enum ArticleStatus
{
    Draft,
    Published,
    Archived
}
