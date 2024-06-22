using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bibtheque.Migrations
{
    /// <inheritdoc />
    public partial class AddCommandeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commande",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quantite = table.Column<int>(type: "int", nullable: false),
                    total = table.Column<double>(type: "float", nullable: false),
                    LivreId = table.Column<int>(type: "int", nullable: false),
                    etat = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commande", x => x.id);
                    table.ForeignKey(
                        name: "FK_Commande_Livre_LivreId",
                        column: x => x.LivreId,
                        principalTable: "Livre",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Commande_Utilisateur_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateur",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commande_LivreId",
                table: "Commande",
                column: "LivreId");

            migrationBuilder.CreateIndex(
                name: "IX_Commande_UtilisateurId",
                table: "Commande",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commande");
        }
    }
}
