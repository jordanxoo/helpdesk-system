using Microsoft.EntityFrameworkCore;
using KnowledgeBaseService.Domain.Entities;

namespace KnowledgeBaseService.Data;

public class KnowledgeBaseDbContext : DbContext
{
    public KnowledgeBaseDbContext(DbContextOptions<KnowledgeBaseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ArticleVote> ArticleVotes => Set<ArticleVote>();
    public DbSet<ArticleView> ArticleViews => Set<ArticleView>();
    public DbSet<ArticleAttachment> ArticleAttachments => Set<ArticleAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

       
        modelBuilder.Entity<Article>(entity =>
        {
            entity.ToTable("articles");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(a => a.Slug)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(a => a.Slug)
                .IsUnique();

            entity.Property(a => a.Content)
                .IsRequired();

            entity.Property(a => a.Summary)
                .HasMaxLength(1000);

            entity.Property(a => a.AuthorName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(a => a.Status)
                .HasConversion<int>();

            entity.HasOne(a => a.Category)
                .WithMany(c => c.Articles)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(a => a.Tags)
                .WithMany(t => t.Articles)
                .UsingEntity(j => j.ToTable("article_tags"));

            entity.HasMany(a => a.Votes)
                .WithOne(v => v.Article)
                .HasForeignKey(v => v.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.Views)
                .WithOne(v => v.Article)
                .HasForeignKey(v => v.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.Attachments)
                .WithOne(a => a.Article)
                .HasForeignKey(a => a.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(c => c.Slug)
                .IsUnique();

            entity.Property(c => c.Description)
                .HasMaxLength(1000);

            entity.Property(c => c.Icon)
                .HasMaxLength(100);

            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.Slug)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(t => t.Slug)
                .IsUnique();

            entity.Property(t => t.Color)
                .HasMaxLength(20);
        });


        modelBuilder.Entity<ArticleVote>(entity =>
        {
            entity.ToTable("article_votes");
            entity.HasKey(v => v.Id);

            entity.HasIndex(v => new { v.ArticleId, v.UserId })
                .IsUnique();
        });

        modelBuilder.Entity<ArticleView>(entity =>
        {
            entity.ToTable("article_views");
            entity.HasKey(v => v.Id);

            entity.Property(v => v.IpAddress)
                .IsRequired()
                .HasMaxLength(45); 

            entity.Property(v => v.UserAgent)
                .HasMaxLength(500);

            entity.HasIndex(v => v.ViewedAt);
        });

        modelBuilder.Entity<ArticleAttachment>(entity =>
        {
            entity.ToTable("article_attachments");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(a => a.StoragePath)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(a => a.DownloadUrl)
                .HasMaxLength(2000);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries();
        
        foreach (var entry in entries)
        {
            if (entry.Entity is Article article)
            {
                if (entry.State == EntityState.Added)
                {
                    article.CreatedAt = DateTime.UtcNow;
                    article.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    article.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is Category category)
            {
                if (entry.State == EntityState.Added)
                {
                    category.CreatedAt = DateTime.UtcNow;
                    category.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    category.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is Tag tag)
            {
                if (entry.State == EntityState.Added)
                {
                    tag.CreatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is ArticleVote vote)
            {
                if (entry.State == EntityState.Added)
                {
                    vote.CreatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is ArticleView view)
            {
                if (entry.State == EntityState.Added)
                {
                    view.ViewedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is ArticleAttachment attachment)
            {
                if (entry.State == EntityState.Added)
                {
                    attachment.UploadedAt = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}