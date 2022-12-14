using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Account.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IncludeUserPropertyInCredential : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Credentials_Accounts_AccountId",
                schema: "Account",
                table: "Credentials");

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                schema: "Account",
                table: "Credentials",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "Account",
                table: "Credentials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Credentials_Accounts_AccountId",
                schema: "Account",
                table: "Credentials",
                column: "AccountId",
                principalSchema: "Account",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Credentials_Accounts_AccountId",
                schema: "Account",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Account",
                table: "Credentials");

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                schema: "Account",
                table: "Credentials",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Credentials_Accounts_AccountId",
                schema: "Account",
                table: "Credentials",
                column: "AccountId",
                principalSchema: "Account",
                principalTable: "Accounts",
                principalColumn: "Id");
        }
    }
}
