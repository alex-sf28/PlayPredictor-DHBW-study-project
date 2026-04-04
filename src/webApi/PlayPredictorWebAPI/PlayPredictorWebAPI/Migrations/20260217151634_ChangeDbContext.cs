using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayPredictorWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalOAuthAccount_Users_UserId",
                table: "ExternalOAuthAccount");

            migrationBuilder.DropForeignKey(
                name: "FK_OAuthToken_ExternalOAuthAccount_ExternalOAuthAccountId",
                table: "OAuthToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OAuthToken",
                table: "OAuthToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExternalOAuthAccount",
                table: "ExternalOAuthAccount");

            migrationBuilder.RenameTable(
                name: "OAuthToken",
                newName: "OAuthTokens");

            migrationBuilder.RenameTable(
                name: "ExternalOAuthAccount",
                newName: "ExternalOAuthAccounts");

            migrationBuilder.RenameIndex(
                name: "IX_OAuthToken_ExternalOAuthAccountId",
                table: "OAuthTokens",
                newName: "IX_OAuthTokens_ExternalOAuthAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_ExternalOAuthAccount_UserId",
                table: "ExternalOAuthAccounts",
                newName: "IX_ExternalOAuthAccounts_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OAuthTokens",
                table: "OAuthTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExternalOAuthAccounts",
                table: "ExternalOAuthAccounts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalOAuthAccounts_Users_UserId",
                table: "ExternalOAuthAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OAuthTokens_ExternalOAuthAccounts_ExternalOAuthAccountId",
                table: "OAuthTokens",
                column: "ExternalOAuthAccountId",
                principalTable: "ExternalOAuthAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalOAuthAccounts_Users_UserId",
                table: "ExternalOAuthAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_OAuthTokens_ExternalOAuthAccounts_ExternalOAuthAccountId",
                table: "OAuthTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OAuthTokens",
                table: "OAuthTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExternalOAuthAccounts",
                table: "ExternalOAuthAccounts");

            migrationBuilder.RenameTable(
                name: "OAuthTokens",
                newName: "OAuthToken");

            migrationBuilder.RenameTable(
                name: "ExternalOAuthAccounts",
                newName: "ExternalOAuthAccount");

            migrationBuilder.RenameIndex(
                name: "IX_OAuthTokens_ExternalOAuthAccountId",
                table: "OAuthToken",
                newName: "IX_OAuthToken_ExternalOAuthAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_ExternalOAuthAccounts_UserId",
                table: "ExternalOAuthAccount",
                newName: "IX_ExternalOAuthAccount_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OAuthToken",
                table: "OAuthToken",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExternalOAuthAccount",
                table: "ExternalOAuthAccount",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalOAuthAccount_Users_UserId",
                table: "ExternalOAuthAccount",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OAuthToken_ExternalOAuthAccount_ExternalOAuthAccountId",
                table: "OAuthToken",
                column: "ExternalOAuthAccountId",
                principalTable: "ExternalOAuthAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
