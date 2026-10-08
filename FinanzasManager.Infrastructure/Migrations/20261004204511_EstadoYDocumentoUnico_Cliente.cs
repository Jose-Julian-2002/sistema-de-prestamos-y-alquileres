using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanzasManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EstadoYDocumentoUnico_Cliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Cliente",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_DocumentoIdentidad",
                table: "Cliente",
                column: "DocumentoIdentidad",
                unique: true,
                filter: "[DocumentoIdentidad] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cliente_DocumentoIdentidad",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Cliente");
        }
    }
}
