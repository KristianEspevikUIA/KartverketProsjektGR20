using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<ObstacleData> Obstacles { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<ObstacleType> ObstacleTypes { get; set; } = null!;
        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<StatusType> StatusTypes { get; set; } = null!;
        public DbSet<AuditEntry> AuditEntries { get; set; } = null!;
        public DbSet<ObstacleComment> ObstacleComments { get; set; } = null!;

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

                entity.HasMany(e => e.Reports)
                    .WithOne(r => r.Obstacle)
                    .HasForeignKey(r => r.ObstacleDataId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Comments)
                    .WithOne(c => c.Obstacle)
                    .HasForeignKey(c => c.ObstacleDataId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(4000);
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<ObstacleComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CommentText).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(256);
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<ObstacleType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<StatusType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            modelBuilder.Entity<AuditEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.EntityId).IsRequired();
                entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PerformedBy).HasMaxLength(256);
                entity.Property(e => e.PerformedAt).IsRequired();
                entity.Property(e => e.Changes).HasMaxLength(4000);
            });
        }
    }
}