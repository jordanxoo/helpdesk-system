using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace AuthService.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Konfiguracja ApplicationUser - Authentication context only
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.RefreshToken)
                .HasMaxLength(500);

            entity.Property(e => e.RefreshTokenExpiryTime);
        });
        
        // Zmiana nazw tabel Identity na lowercase (lepiej dla PostgreSQL)
        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("user_roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("user_claims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("user_logins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("role_claims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("user_tokens");

        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();

        
    }
}
