using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayPredictorWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSensitiveData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CalendarEvents");

            migrationBuilder.RenameColumn(
                name: "Summary",
                table: "CalendarEvents",
                newName: "EncryptedSummary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EncryptedSummary",
                table: "CalendarEvents",
                newName: "Summary");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CalendarEvents",
                type: "text",
                nullable: true);
        }
    }
}
