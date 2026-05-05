using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InovaGAB.API.Migrations
{
    /// <inheritdoc />
    public partial class FixTyposInvestmentProductivityGain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Investiment",
                table: "Projects",
                newName: "Investment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Investment",
                table: "Projects",
                newName: "Investiment");
        }
    }
}
