using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationToObstacle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "Obstacles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Organization",
                table: "Obstacles");
        }
    }
}
