using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InovaGAB.API.Migrations
{
    /// <inheritdoc />
    public partial class FixIdeaChallengeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChallengId",
                table: "Idea");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChallengId",
                table: "Idea",
                type: "integer",
                nullable: true);
        }
    }
}
