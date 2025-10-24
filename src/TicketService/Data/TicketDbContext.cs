using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace TicketService.Data;

public class TicketDbContext : DbContext
{
    public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<TicketComment> TicketComments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfiguracja tabeli Tickets
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("tickets");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(t => t.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(t => t.Description)
                .HasColumnName("description")
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(t => t.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(t => t.Priority)
                .HasColumnName("priority")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(t => t.Category)
                .HasColumnName("category")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(t => t.CustomerId)
                .HasColumnName("customer_id")
                .IsRequired();

            entity.Property(t => t.AssignedAgentId)
                .HasColumnName("assigned_agent_id");

            entity.Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(t => t.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(t => t.ResolvedAt)
                .HasColumnName("resolved_at");

            // Indeksy dla lepszej wydajności
            entity.HasIndex(t => t.CustomerId)
                .HasDatabaseName("ix_tickets_customer_id");

            entity.HasIndex(t => t.AssignedAgentId)
                .HasDatabaseName("ix_tickets_assigned_agent_id");

            entity.HasIndex(t => t.Status)
                .HasDatabaseName("ix_tickets_status");

            entity.HasIndex(t => t.Priority)
                .HasDatabaseName("ix_tickets_priority");

            entity.HasIndex(t => t.CreatedAt)
                .HasDatabaseName("ix_tickets_created_at");

            // Relacja z komentarzami - ignorujemy nawigację
            entity.Ignore(t => t.Comments);
        });

        // Konfiguracja tabeli TicketComments
        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.ToTable("ticket_comments");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(c => c.TicketId)
                .HasColumnName("ticket_id")
                .IsRequired();

            entity.Property(c => c.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(c => c.Content)
                .HasColumnName("content")
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(c => c.IsInternal)
                .HasColumnName("is_internal")
                .IsRequired();

            entity.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // Indeksy
            entity.HasIndex(c => c.TicketId)
                .HasDatabaseName("ix_ticket_comments_ticket_id");

            entity.HasIndex(c => c.UserId)
                .HasDatabaseName("ix_ticket_comments_user_id");

            entity.HasIndex(c => c.CreatedAt)
                .HasDatabaseName("ix_ticket_comments_created_at");
        });
    }
}