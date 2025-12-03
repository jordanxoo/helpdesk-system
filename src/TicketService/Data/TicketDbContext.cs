using Microsoft.EntityFrameworkCore;
using Shared.Models;
using MassTransit;
namespace TicketService.Data;

public class TicketDbContext : DbContext
{
    public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<TicketComment> TicketComments { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<SLA> Slas { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<TicketAttachment> TicketAttachments { get; set; } = null!;
    public DbSet<TicketAuditLog> TicketAuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

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

            entity.Property(t => t.OrganizationId)
                .HasColumnName("organization_id");

            entity.Property(t => t.SlaId)
                .HasColumnName("sla_id");

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

            entity.HasIndex(t => t.OrganizationId)
                .HasDatabaseName("ix_tickets_organization_id");

            entity.HasIndex(t => t.SlaId)
                .HasDatabaseName("ix_tickets_sla_id");

            // Relacje
            entity.HasOne(t => t.Organization)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Sla)
                .WithMany()
                .HasForeignKey(t => t.SlaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(t => t.Comments)
                .WithOne(c => c.Ticket)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Attachments)
                .WithOne(a => a.Ticket)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.AuditLogs)
                .WithOne(a => a.Ticket)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many z Tag przez tabelę łączącą
            entity.HasMany(t => t.Tags)
                .WithMany(tag => tag.Tickets)
                .UsingEntity<Dictionary<string, object>>(
                    "ticket_tags",
                    j => j.HasOne<Tag>().WithMany().HasForeignKey("tag_id"),
                    j => j.HasOne<Ticket>().WithMany().HasForeignKey("ticket_id")
                );
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

        // Konfiguracja tabeli Organizations
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(o => o.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(o => o.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            entity.Property(o => o.ContactEmail)
                .HasColumnName("contact_email")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.ContactPhone)
                .HasColumnName("contact_phone")
                .HasMaxLength(20);

            entity.Property(o => o.IsActive)
                .HasColumnName("is_active")
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(o => o.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(o => o.SlaId)
                .HasColumnName("sla_id");

            // Indeksy
            entity.HasIndex(o => o.Name)
                .HasDatabaseName("ix_organizations_name");

            entity.HasIndex(o => o.IsActive)
                .HasDatabaseName("ix_organizations_is_active");

            entity.HasIndex(o => o.SlaId)
                .HasDatabaseName("ix_organizations_sla_id");

            // Relacja z SLA
            entity.HasOne(o => o.Sla)
                .WithMany(s => s.Organizations)
                .HasForeignKey(o => o.SlaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Konfiguracja tabeli SLAs
        modelBuilder.Entity<SLA>(entity =>
        {
            entity.ToTable("slas");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(s => s.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            entity.Property(s => s.ResponseTimeCritical)
                .HasColumnName("response_time_critical")
                .IsRequired();

            entity.Property(s => s.ResponseTimeHigh)
                .HasColumnName("response_time_high")
                .IsRequired();

            entity.Property(s => s.ResponseTimeMedium)
                .HasColumnName("response_time_medium")
                .IsRequired();

            entity.Property(s => s.ResponseTimeLow)
                .HasColumnName("response_time_low")
                .IsRequired();

            entity.Property(s => s.ResolutionTimeCritical)
                .HasColumnName("resolution_time_critical")
                .IsRequired();

            entity.Property(s => s.ResolutionTimeHigh)
                .HasColumnName("resolution_time_high")
                .IsRequired();

            entity.Property(s => s.ResolutionTimeMedium)
                .HasColumnName("resolution_time_medium")
                .IsRequired();

            entity.Property(s => s.ResolutionTimeLow)
                .HasColumnName("resolution_time_low")
                .IsRequired();

            entity.Property(s => s.IsActive)
                .HasColumnName("is_active")
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(s => s.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(s => s.UpdatedAt)
                .HasColumnName("updated_at");

            // Indeksy
            entity.HasIndex(s => s.Name)
                .HasDatabaseName("ix_slas_name");

            entity.HasIndex(s => s.IsActive)
                .HasDatabaseName("ix_slas_is_active");
        });

        // Konfiguracja tabeli Tags
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(t => t.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(t => t.Color)
                .HasColumnName("color")
                .HasMaxLength(7); // #RRGGBB

            entity.Property(t => t.Description)
                .HasColumnName("description")
                .HasMaxLength(200);

            entity.Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // Indeksy
            entity.HasIndex(t => t.Name)
                .HasDatabaseName("ix_tags_name")
                .IsUnique();
        });

        // Konfiguracja tabeli TicketAttachments
        modelBuilder.Entity<TicketAttachment>(entity =>
        {
            entity.ToTable("ticket_attachments");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(a => a.TicketId)
                .HasColumnName("ticket_id")
                .IsRequired();

            entity.Property(a => a.UploadedById)
                .HasColumnName("uploaded_by_id")
                .IsRequired();

            entity.Property(a => a.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(a => a.ContentType)
                .HasColumnName("content_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(a => a.FileSizeBytes)
                .HasColumnName("file_size_bytes")
                .IsRequired();

            entity.Property(a => a.StoragePath)
                .HasColumnName("storage_path")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(a => a.DownloadUrl)
                .HasColumnName("download_url")
                .HasMaxLength(1000);

            entity.Property(a => a.UploadedAt)
                .HasColumnName("uploaded_at")
                .IsRequired();

            // Indeksy
            entity.HasIndex(a => a.TicketId)
                .HasDatabaseName("ix_ticket_attachments_ticket_id");

            entity.HasIndex(a => a.UploadedById)
                .HasDatabaseName("ix_ticket_attachments_uploaded_by_id");

            entity.HasIndex(a => a.UploadedAt)
                .HasDatabaseName("ix_ticket_attachments_uploaded_at");
        });

        // Konfiguracja tabeli TicketAuditLogs
        modelBuilder.Entity<TicketAuditLog>(entity =>
        {
            entity.ToTable("ticket_audit_logs");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(a => a.TicketId)
                .HasColumnName("ticket_id")
                .IsRequired();

            entity.Property(a => a.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(a => a.Action)
                .HasColumnName("action")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(a => a.FieldName)
                .HasColumnName("field_name")
                .HasMaxLength(100);

            entity.Property(a => a.OldValue)
                .HasColumnName("old_value")
                .HasMaxLength(500);

            entity.Property(a => a.NewValue)
                .HasColumnName("new_value")
                .HasMaxLength(500);

            entity.Property(a => a.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            entity.Property(a => a.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // Indeksy
            entity.HasIndex(a => a.TicketId)
                .HasDatabaseName("ix_ticket_audit_logs_ticket_id");

            entity.HasIndex(a => a.UserId)
                .HasDatabaseName("ix_ticket_audit_logs_user_id");

            entity.HasIndex(a => a.Action)
                .HasDatabaseName("ix_ticket_audit_logs_action");

            entity.HasIndex(a => a.CreatedAt)
                .HasDatabaseName("ix_ticket_audit_logs_created_at");
        });
    }
}