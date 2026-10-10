using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanzasManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Pago_ClienteCabecera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Pago",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Pago_ClienteId",
                table: "Pago",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pago_Cliente_ClienteId",
                table: "Pago",
                column: "ClienteId",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pago_Cliente_ClienteId",
                table: "Pago");

            migrationBuilder.DropIndex(
                name: "IX_Pago_ClienteId",
                table: "Pago");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Pago");
        }
    }
}
