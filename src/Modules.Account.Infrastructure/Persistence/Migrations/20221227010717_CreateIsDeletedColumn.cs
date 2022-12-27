using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Account.Infrastructure.Persistence.Migrations
{
    public partial class CreateIsDeletedColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Account",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Id_IsDeleted",
                schema: "Account",
                table: "Accounts",
                columns: new[] { "Id", "IsDeleted" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_Id_IsDeleted",
                schema: "Account",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Account",
                table: "Accounts");
        }
    }
}
