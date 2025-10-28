using System;
using FirstWebApplication1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FirstWebApplication1.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder
                .UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity("FirstWebApplication1.Models.ObstacleData", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<double?>("Latitude")
                    .HasColumnType("double");

                b.Property<string>("LineGeoJson")
                    .HasColumnType("longtext")
                    .HasAnnotation("MySql:CharSet", "utf8mb4");

                b.Property<double?>("Longitude")
                    .HasColumnType("double");

                b.Property<double>("ObstacleHeight")
                    .HasColumnType("double");

                b.Property<string>("ObstacleDescription")
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnType("varchar(1000)")
                    .HasAnnotation("MySql:CharSet", "utf8mb4");

                b.Property<string>("ObstacleName")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)")
                    .HasAnnotation("MySql:CharSet", "utf8mb4");

                b.HasKey("Id");

                b.ToTable("Obstacles")
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_0900_ai_ci");
            });
#pragma warning restore 612, 618
        }
    }
}
