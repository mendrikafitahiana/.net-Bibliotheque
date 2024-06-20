using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bibtheque.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLivreStockRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stock_Livre_LivreId",
                table: "Stock");

            migrationBuilder.AddForeignKey(
                name: "FK_Stock_Livre_LivreId",
                table: "Stock",
                column: "LivreId",
                principalTable: "Livre",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stock_Livre_LivreId",
                table: "Stock");

            migrationBuilder.AddForeignKey(
                name: "FK_Stock_Livre_LivreId",
                table: "Stock",
                column: "LivreId",
                principalTable: "Livre",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
