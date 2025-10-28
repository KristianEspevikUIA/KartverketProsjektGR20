using FirstWebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ObstacleData> Obstacles => Set<ObstacleData>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
