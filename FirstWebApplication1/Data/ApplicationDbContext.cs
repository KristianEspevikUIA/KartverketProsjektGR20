using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        // DbSet som representerer tabellen i databasen hvor alle hindere lagres
        public DbSet<ObstacleData> Obstacles { get; set; } = null!;

        // Konstruktør brukt av Dependency Injection og EF Core til å konfigurere databasen
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Viktig for at Identity kan generere sine egne tabeller og regler
            base.OnModelCreating(modelBuilder);

            // Modellkonfigurasjon for ObstacleData-tabellen
            modelBuilder.Entity<ObstacleData>(entity =>
            {
                entity.HasKey(e => e.Id); // Primærnøkkel i databasen

                entity.Property(e => e.ObstacleName).HasMaxLength(100); // Begrensning for feltstørrelse
                entity.Property(e => e.ObstacleName).IsRequired(); // Må fylles ut

                entity.Property(e => e.ObstacleHeight).IsRequired(); // Ikke lov med null-verdi

                entity.Property(e => e.ObstacleDescription).HasMaxLength(1000); // Maks tegnlengde i beskrivelse
                entity.Property(e => e.ObstacleDescription).IsRequired(); // Beskrivelse påkrevd

                entity.Property(e => e.LineGeoJson).HasColumnType("longtext"); 
                // Lagres som longtext for å håndtere store datastrenger (GeoJSON)
            });
        }
    }
}