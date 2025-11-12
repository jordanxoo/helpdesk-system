using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }


    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").IsRequired();

            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100).IsRequired();

            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20).IsRequired();
            
            entity.Property(e => e.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);

            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("idx_users_email");
            entity.HasIndex(e => e.Role).HasDatabaseName("idx_users_role");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_users_is_active");
            entity.Ignore(e => e.FullName);
        });
    }
}
