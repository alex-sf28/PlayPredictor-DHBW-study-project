using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayPredictorWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOAuthAccount2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalOAuthAccount_OAuthToken_TokenId",
                table: "ExternalOAuthAccount");

            migrationBuilder.DropIndex(
                name: "IX_ExternalOAuthAccount_TokenId",
                table: "ExternalOAuthAccount");

            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "ExternalOAuthAccount");

            migrationBuilder.AddColumn<int>(
                name: "ExternalOAuthAccountId",
                table: "OAuthToken",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OAuthToken_ExternalOAuthAccountId",
                table: "OAuthToken",
                column: "ExternalOAuthAccountId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OAuthToken_ExternalOAuthAccount_ExternalOAuthAccountId",
                table: "OAuthToken",
                column: "ExternalOAuthAccountId",
                principalTable: "ExternalOAuthAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OAuthToken_ExternalOAuthAccount_ExternalOAuthAccountId",
                table: "OAuthToken");

            migrationBuilder.DropIndex(
                name: "IX_OAuthToken_ExternalOAuthAccountId",
                table: "OAuthToken");

            migrationBuilder.DropColumn(
                name: "ExternalOAuthAccountId",
                table: "OAuthToken");

            migrationBuilder.AddColumn<int>(
                name: "TokenId",
                table: "ExternalOAuthAccount",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalOAuthAccount_TokenId",
                table: "ExternalOAuthAccount",
                column: "TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalOAuthAccount_OAuthToken_TokenId",
                table: "ExternalOAuthAccount",
                column: "TokenId",
                principalTable: "OAuthToken",
                principalColumn: "Id");
        }
    }
}
