using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceFlow.Modules.Transactions.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstallmentCount",
                schema: "transactions",
                table: "transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstallmentGroupId",
                schema: "transactions",
                table: "transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentNumber",
                schema: "transactions",
                table: "transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactions_InstallmentGroupId",
                schema: "transactions",
                table: "transactions",
                column: "InstallmentGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transactions_InstallmentGroupId",
                schema: "transactions",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "InstallmentCount",
                schema: "transactions",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "InstallmentGroupId",
                schema: "transactions",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "InstallmentNumber",
                schema: "transactions",
                table: "transactions");
        }
    }
}
