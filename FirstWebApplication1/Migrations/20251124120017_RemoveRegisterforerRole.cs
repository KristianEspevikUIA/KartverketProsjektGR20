using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication1.Migrations
{
    /// <summary>
    /// Removes the obsolete "Registerfører" role and reassigns users to the Caseworker role. Executed as
    /// raw SQL to preserve data integrity during the migration.
    /// </summary>
    public partial class RemoveRegisterforerRole : Migration
    {
        /// <summary>
        /// Executes SQL to move users off the removed role and delete it.
        /// </summary>
        /// <param name="migrationBuilder">EF Core migration builder.</param>
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

        /// <summary>
        /// Re-creates the removed role on downgrade (does not reassign users back).
        /// </summary>
        /// <param name="migrationBuilder">EF Core migration builder.</param>
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
