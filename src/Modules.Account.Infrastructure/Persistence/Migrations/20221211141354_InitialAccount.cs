using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Account.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Account");

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "Account",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NickName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Credentials",
                schema: "Account",
                columns: table => new
                {
                    AuthenticationProvider = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => new { x.AuthenticationProvider, x.ProviderId });
                    table.ForeignKey(
                        name: "FK_Credentials_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "Account",
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_AccountId",
                schema: "Account",
                table: "Credentials",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credentials",
                schema: "Account");

            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "Account");
        }
    }
}
