using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bibtheque.Migrations
{
    /// <inheritdoc />
    public partial class AddPaiementModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Paiement",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommandeId = table.Column<int>(type: "int", nullable: false),
                    numeroCarte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    dateLivraison = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiement", x => x.id);
                    table.ForeignKey(
                        name: "FK_Paiement_Commande_CommandeId",
                        column: x => x.CommandeId,
                        principalTable: "Commande",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Paiement_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Region",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Paiement_CommandeId",
                table: "Paiement",
                column: "CommandeId");

            migrationBuilder.CreateIndex(
                name: "IX_Paiement_RegionId",
                table: "Paiement",
                column: "RegionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Paiement");
        }
    }
}
