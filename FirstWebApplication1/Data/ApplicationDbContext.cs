using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication1.Data
{
    /// <summary>
    /// EF Core database context that combines ASP.NET Core Identity tables with the domain entity
    /// <see cref="ObstacleData"/>. Acts as the bridge between controllers (MVC) and the SQL database.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        /// <summary>
        /// Table for all obstacle reports. EF Core uses this DbSet to compose parameterized SQL queries,
        /// protecting against SQL injection while enabling LINQ queries in controllers/services.
        /// </summary>
        public DbSet<ObstacleData> Obstacles { get; set; } = null!;

        /// <summary>
        /// DI-friendly constructor that forwards DbContextOptions to the Identity base class so Identity
        /// can configure its own schema.
        /// </summary>
        /// <param name="options">Options built in Program.cs with the MariaDB connection.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Configures the EF Core model. Base.OnModelCreating is required to ensure Identity tables are
        /// mapped. ObstacleData column rules are defined here to enforce validation at the database level.
        /// </summary>
        /// <param name="modelBuilder">Fluent builder used by EF Core during migrations.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Important: call base so Identity can configure its schema and authorization tables.
            base.OnModelCreating(modelBuilder);

            // Configure ObstacleData columns and constraints for consistency with MVC validation.
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