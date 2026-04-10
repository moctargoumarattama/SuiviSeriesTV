using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuiviSeriesTV.Models;

namespace SuiviSeriesTV.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Serie> Series => Set<Serie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Serie>(entity =>
        {
            entity.Property(s => s.Title).IsRequired().HasMaxLength(120);
            entity.Property(s => s.Genre).IsRequired().HasMaxLength(60);
            entity.Property(s => s.Description).IsRequired().HasMaxLength(1000);
            entity.Property(s => s.StreamingPlatform).IsRequired().HasMaxLength(80);
            entity.Property(s => s.PosterUrl).HasMaxLength(400);
            entity.Property(s => s.BackdropUrl).HasMaxLength(400);
            entity.Property(s => s.PersonalComment).HasMaxLength(1200);
            entity.Property(s => s.PersonalRating).HasDefaultValue(0);
            entity.Property(s => s.DateAdded).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(s => s.Owner)
                .WithMany()
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(s => new { s.OwnerId, s.Title });
            entity.HasIndex(s => new { s.OwnerId, s.Status, s.ContentType });
            entity.HasIndex(s => s.NextReleaseDate);
        });
    }
}
