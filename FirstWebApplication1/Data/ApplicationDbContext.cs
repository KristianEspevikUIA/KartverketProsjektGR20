using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        // Single DbSet for obstacles
        public DbSet<ObstacleData> Obstacles { get; set; } = null!;

        // Keep only the DbContextOptions constructor used by DI/EF
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Important: call base so Identity can configure its schema
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ObstacleData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ObstacleName).HasMaxLength(100);
                entity.Property(e => e.ObstacleName).IsRequired();
                entity.Property(e => e.ObstacleHeight).IsRequired();
                entity.Property(e => e.ObstacleDescription).HasMaxLength(1000);
                entity.Property(e => e.ObstacleDescription).IsRequired();
                entity.Property(e => e.LineGeoJson).HasColumnType("longtext");
            });
        }
    }
}