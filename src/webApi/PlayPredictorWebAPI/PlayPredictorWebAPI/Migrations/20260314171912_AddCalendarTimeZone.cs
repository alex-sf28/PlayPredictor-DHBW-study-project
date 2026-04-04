using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayPredictorWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastStartDate",
                table: "Calendars",
                newName: "LastStartDateUtc");

            migrationBuilder.RenameColumn(
                name: "LastEndDate",
                table: "Calendars",
                newName: "LastEndDateUtc");

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Calendars",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Calendars");

            migrationBuilder.RenameColumn(
                name: "LastStartDateUtc",
                table: "Calendars",
                newName: "LastStartDate");

            migrationBuilder.RenameColumn(
                name: "LastEndDateUtc",
                table: "Calendars",
                newName: "LastEndDate");
        }
    }
}
