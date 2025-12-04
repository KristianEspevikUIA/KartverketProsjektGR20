using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRegisterforerRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, find the IDs of the roles we are working with.
            // Note: In a real-world scenario, you might want to add error handling here in case the roles don't exist.
            migrationBuilder.Sql(@"
                SET @caseworkerRoleId = (SELECT Id FROM AspNetRoles WHERE NormalizedName = 'CASEWORKER');
                SET @registerforerRoleId = (SELECT Id FROM AspNetRoles WHERE NormalizedName = 'REGISTERFØRER');
            ");

            // Update all users who have the 'Registerfører' role.
            // Change their RoleId to the 'Caseworker' RoleId.
            // This ensures no user is left without a role.
            migrationBuilder.Sql(@"
                UPDATE AspNetUserRoles
                SET RoleId = @caseworkerRoleId
                WHERE RoleId = @registerforerRoleId;
            ");

            // After reassigning the users, delete the now-unused 'Registerfører' role.
            migrationBuilder.Sql(@"
                DELETE FROM AspNetRoles
                WHERE Id = @registerforerRoleId;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // To make this migration reversible, the Down method should re-create the 'Registerfører' role.
            // This is generally good practice but may not be strictly necessary if you don't plan to revert.
            migrationBuilder.Sql(@"
                INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                VALUES (UUID(), 'Registerfører', 'REGISTERFØRER', UUID());
            ");

            // Note: The Down method does not automatically reassign users back from Caseworker to Registerfører,
            // as it would be complex to know which users were originally in that role.
        }
    }
}
