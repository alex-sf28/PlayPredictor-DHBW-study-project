using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayPredictorWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMatchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Matches",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_FaceitAccountId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Matches");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Matches",
                table: "Matches",
                columns: new[] { "FaceitAccountId", "MatchId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Matches",
                table: "Matches");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Matches",
                table: "Matches",
                columns: new[] { "Id", "MatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_FaceitAccountId",
                table: "Matches",
                column: "FaceitAccountId");
        }
    }
}
