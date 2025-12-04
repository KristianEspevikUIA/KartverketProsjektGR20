using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication1.Migrations
{
    /// <summary>
    /// Adds the ObstacleType column to categorize obstacles in the database.
    /// </summary>
    public partial class AddObstacleTypeColumn : Migration
    {
        /// <summary>
        /// Applies schema changes to add ObstacleType with length constraints.
        /// </summary>
        /// <param name="migrationBuilder">EF Core migration builder.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ObstacleType",
                table: "Obstacles",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <summary>
        /// Removes the ObstacleType column when rolling back.
        /// </summary>
        /// <param name="migrationBuilder">EF Core migration builder.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObstacleType",
                table: "Obstacles");
        }
    }
}
