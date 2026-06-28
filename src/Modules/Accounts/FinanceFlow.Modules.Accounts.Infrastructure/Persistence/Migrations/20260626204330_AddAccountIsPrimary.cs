using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceFlow.Modules.Accounts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountIsPrimary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                schema: "accounts",
                table: "accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrimary",
                schema: "accounts",
                table: "accounts");
        }
    }
}
