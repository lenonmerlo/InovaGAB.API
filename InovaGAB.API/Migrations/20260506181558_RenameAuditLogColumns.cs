using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InovaGAB.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameAuditLogColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Role",
                table: "AuditLogs",
                newName: "UserRole");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "AuditLogs",
                newName: "UserEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserRole",
                table: "AuditLogs",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "AuditLogs",
                newName: "Email");
        }
    }
}
