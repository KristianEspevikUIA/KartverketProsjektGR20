using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddObstacleStatusTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Obstacles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Obstacles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeclineReason",
                table: "Obstacles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DeclinedBy",
                table: "Obstacles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeclinedDate",
                table: "Obstacles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Obstacles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Obstacles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Obstacles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SubmittedBy",
                table: "Obstacles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedDate",
                table: "Obstacles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "DeclineReason",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "DeclinedBy",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "DeclinedDate",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "SubmittedDate",
                table: "Obstacles");
        }
    }
}
