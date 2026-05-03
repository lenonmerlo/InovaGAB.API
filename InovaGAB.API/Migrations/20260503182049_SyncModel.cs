using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InovaGAB.API.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenge_User_CreatedById",
                table: "Challenge");

            migrationBuilder.DropForeignKey(
                name: "FK_Idea_Challenge_ChallengeId",
                table: "Idea");

            migrationBuilder.DropForeignKey(
                name: "FK_Idea_User_UserId",
                table: "Idea");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Idea_IdeaId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_User_ManagerId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_StrategicGuideline_User_CreatedById",
                table: "StrategicGuideline");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StrategicGuideline",
                table: "StrategicGuideline");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project",
                table: "Project");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Idea",
                table: "Idea");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Challenge",
                table: "Challenge");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "StrategicGuideline",
                newName: "StrategicGuidelines");

            migrationBuilder.RenameTable(
                name: "Project",
                newName: "Projects");

            migrationBuilder.RenameTable(
                name: "Idea",
                newName: "Ideas");

            migrationBuilder.RenameTable(
                name: "Challenge",
                newName: "Challenges");

            migrationBuilder.RenameIndex(
                name: "IX_StrategicGuideline_CreatedById",
                table: "StrategicGuidelines",
                newName: "IX_StrategicGuidelines_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Project_ManagerId",
                table: "Projects",
                newName: "IX_Projects_ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Project_IdeaId",
                table: "Projects",
                newName: "IX_Projects_IdeaId");

            migrationBuilder.RenameIndex(
                name: "IX_Idea_UserId",
                table: "Ideas",
                newName: "IX_Ideas_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Idea_ChallengeId",
                table: "Ideas",
                newName: "IX_Ideas_ChallengeId");

            migrationBuilder.RenameIndex(
                name: "IX_Challenge_CreatedById",
                table: "Challenges",
                newName: "IX_Challenges_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StrategicGuidelines",
                table: "StrategicGuidelines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ideas",
                table: "Ideas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Challenges",
                table: "Challenges",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Users_CreatedById",
                table: "Challenges",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Challenges_ChallengeId",
                table: "Ideas",
                column: "ChallengeId",
                principalTable: "Challenges",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Users_UserId",
                table: "Ideas",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Ideas_IdeaId",
                table: "Projects",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_ManagerId",
                table: "Projects",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StrategicGuidelines_Users_CreatedById",
                table: "StrategicGuidelines",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Users_CreatedById",
                table: "Challenges");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_Challenges_ChallengeId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_Users_UserId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Ideas_IdeaId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_ManagerId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_StrategicGuidelines_Users_CreatedById",
                table: "StrategicGuidelines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StrategicGuidelines",
                table: "StrategicGuidelines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ideas",
                table: "Ideas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Challenges",
                table: "Challenges");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "StrategicGuidelines",
                newName: "StrategicGuideline");

            migrationBuilder.RenameTable(
                name: "Projects",
                newName: "Project");

            migrationBuilder.RenameTable(
                name: "Ideas",
                newName: "Idea");

            migrationBuilder.RenameTable(
                name: "Challenges",
                newName: "Challenge");

            migrationBuilder.RenameIndex(
                name: "IX_StrategicGuidelines_CreatedById",
                table: "StrategicGuideline",
                newName: "IX_StrategicGuideline_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_ManagerId",
                table: "Project",
                newName: "IX_Project_ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_IdeaId",
                table: "Project",
                newName: "IX_Project_IdeaId");

            migrationBuilder.RenameIndex(
                name: "IX_Ideas_UserId",
                table: "Idea",
                newName: "IX_Idea_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Ideas_ChallengeId",
                table: "Idea",
                newName: "IX_Idea_ChallengeId");

            migrationBuilder.RenameIndex(
                name: "IX_Challenges_CreatedById",
                table: "Challenge",
                newName: "IX_Challenge_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StrategicGuideline",
                table: "StrategicGuideline",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Idea",
                table: "Idea",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Challenge",
                table: "Challenge",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenge_User_CreatedById",
                table: "Challenge",
                column: "CreatedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Idea_Challenge_ChallengeId",
                table: "Idea",
                column: "ChallengeId",
                principalTable: "Challenge",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Idea_User_UserId",
                table: "Idea",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Idea_IdeaId",
                table: "Project",
                column: "IdeaId",
                principalTable: "Idea",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_User_ManagerId",
                table: "Project",
                column: "ManagerId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StrategicGuideline_User_CreatedById",
                table: "StrategicGuideline",
                column: "CreatedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
