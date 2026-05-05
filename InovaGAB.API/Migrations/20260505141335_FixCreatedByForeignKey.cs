using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InovaGAB.API.Migrations
{
    /// <inheritdoc />
    public partial class FixCreatedByForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""StrategicGuidelines""
                    DROP CONSTRAINT IF EXISTS ""FK_StrategicGuidelines_Users_CreatedById"";

                ALTER TABLE ""StrategicGuidelines""
                    RENAME COLUMN ""CreatedById"" TO ""CreatedByUserId"";

                ALTER TABLE ""StrategicGuidelines""
                    ADD CONSTRAINT ""FK_StrategicGuidelines_Users_CreatedByUserId""
                    FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users""(""Id"") ON DELETE CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""StrategicGuidelines""
                    DROP CONSTRAINT IF EXISTS ""FK_StrategicGuidelines_Users_CreatedByUserId"";

                ALTER TABLE ""StrategicGuidelines""
                    RENAME COLUMN ""CreatedByUserId"" TO ""CreatedById"";

                ALTER TABLE ""StrategicGuidelines""
                    ADD CONSTRAINT ""FK_StrategicGuidelines_Users_CreatedById""
                    FOREIGN KEY (""CreatedById"") REFERENCES ""Users""(""Id"") ON DELETE CASCADE;
            ");
        }
    }
}
