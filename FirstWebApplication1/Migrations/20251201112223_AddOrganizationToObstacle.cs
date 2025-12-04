using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication1.Migrations
{
    /// <summary>
    /// Adds an Organization column to Obstacles to capture submitter affiliation for filtering/auditing.
    /// </summary>
    public partial class AddOrganizationToObstacle : Migration
    {
        /// <summary>
        /// Applies the Organization column to the table.
        /// </summary>
        /// <param name="migrationBuilder">EF Core migration builder.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "Obstacles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <summary>
        /// Removes the Organization column on downgrade.
        /// </summary>
        /// <param name="migrationBuilder">EF Core migration builder.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Organization",
                table: "Obstacles");
        }
    }
}
