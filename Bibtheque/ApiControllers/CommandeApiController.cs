using Bibtheque.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using NuGet.Common;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Bibtheque.ApiControllers
{
    [Route("api/commande")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CommandeApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CommandeApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost("addCommande")]
        public IActionResult AjouterCommande([FromBody] JsonElement commande)
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.PrimarySid));

            if (string.IsNullOrEmpty(userId.ToString()))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            if (commande.TryGetProperty("idLivre", out JsonElement livreElement))
            {
                int idLivre = livreElement.GetInt32();

                Commande commandeInsere = new Commande();

                string connectionString = _configuration.GetConnectionString("BibthequeContext");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string checkLivreQuery = "SELECT COUNT(*) FROM Livre WHERE id = @LivreId";
                    using (SqlCommand checkLivreCmd = new SqlCommand(checkLivreQuery, connection))
                    {
                        checkLivreCmd.Parameters.AddWithValue("@LivreId", idLivre);
                        int livreCount = (int)checkLivreCmd.ExecuteScalar();
                        if (livreCount == 0)
                        {
                            return NotFound("Le livre spécifié n'existe pas.");
                        }
                    }

                    string checkLivre2Query = "SELECT COUNT(*) FROM Commande WHERE LivreId = @LivreId";
                    using (SqlCommand checkLivre2Cmd = new SqlCommand(checkLivre2Query, connection))
                    {
                        checkLivre2Cmd.Parameters.AddWithValue("@LivreId", idLivre);
                        int livreCount = (int)checkLivre2Cmd.ExecuteScalar();
                        if (livreCount > 0)
                        {
                            return BadRequest($"Livre déjà commandé");
                        }
                    }

                    commandeInsere.LivreId = idLivre;
                    commandeInsere.quantite = 1;
                    commandeInsere.UtilisateurId = userId;
                    commandeInsere.dateCommande = DateOnly.FromDateTime(DateTime.Now);

                    string getStockQuery = "SELECT quantite FROM Stock WHERE LivreId = @LivreId";
                    using (SqlCommand getStockCmd = new SqlCommand(getStockQuery, connection))
                    {
                        getStockCmd.Parameters.AddWithValue("@LivreId", idLivre);
                        int stock = (int)getStockCmd.ExecuteScalar();
                        if(commandeInsere.quantite > stock)
                        {
                            return BadRequest($"Stock insuffisant. Stock disponible : {stock}");
                        }
                    }

                    string getPrixQuery = "SELECT prix FROM Livre WHERE id = @LivreId";
                    using (SqlCommand getPrixCmd = new SqlCommand(getPrixQuery, connection))
                    {
                        getPrixCmd.Parameters.AddWithValue("@LivreId", idLivre);
                        double prix = (double)getPrixCmd.ExecuteScalar();
                        commandeInsere.total = prix * commandeInsere.quantite;
                    }

                    string insertQuery = @"INSERT INTO Commande (quantite, total, LivreId, etat, UtilisateurId, dateCommande) 
                                       VALUES (@quantite, @total, @LivreId, @etat, @utilisateur, @dateCommande);
                                       SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@quantite", commandeInsere.quantite);
                        cmd.Parameters.AddWithValue("@total", commandeInsere.total);
                        cmd.Parameters.AddWithValue("@LivreId", commandeInsere.LivreId);
                        cmd.Parameters.AddWithValue("@etat", 0); // État initial : panier
                        cmd.Parameters.AddWithValue("@utilisateur", userId);
                        cmd.Parameters.AddWithValue("@dateCommande", commandeInsere.dateCommande);

                        commandeInsere.id = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                }
                return Ok(new { commande = commandeInsere });
            }
            return BadRequest("Invalid Commande request");
        }

        [HttpPost("validCommande")]
        public IActionResult ValiderCommande([FromBody] JsonElement commande, int userId)
        {
            //int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.PrimarySid));

            if (string.IsNullOrEmpty(userId.ToString()))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            if (commande.TryGetProperty("idLivre", out JsonElement livreElement) && 
                commande.TryGetProperty("quantite", out JsonElement quantiteElement) &&
                commande.TryGetProperty("idCommande", out JsonElement commandeElement)
                )
            {
                int idLivre = livreElement.GetInt32();
                int quantite = quantiteElement.GetInt32();
                int idCommande = commandeElement.GetInt32();

                Commande commandeInsere = new Commande();

                string connectionString = _configuration.GetConnectionString("BibthequeContext");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string checkLivreQuery = "SELECT COUNT(*) FROM Livre WHERE id = @LivreId";
                    using (SqlCommand checkLivreCmd = new SqlCommand(checkLivreQuery, connection))
                    {
                        checkLivreCmd.Parameters.AddWithValue("@LivreId", idLivre);
                        int livreCount = (int)checkLivreCmd.ExecuteScalar();
                        if (livreCount == 0)
                        {
                            return NotFound("Le livre spécifié n'existe pas.");
                        }
                    }

                    commandeInsere.LivreId = idLivre;
                    commandeInsere.quantite = quantite;
                    commandeInsere.UtilisateurId = userId;
                    commandeInsere.id = idCommande;
                    commandeInsere.dateCommande = DateOnly.FromDateTime(DateTime.Now);

                    string getStockQuery = "SELECT quantite FROM Stock WHERE LivreId = @LivreId";
                    using (SqlCommand getStockCmd = new SqlCommand(getStockQuery, connection))
                    {
                        getStockCmd.Parameters.AddWithValue("@LivreId", idLivre);
                        int stock = (int)getStockCmd.ExecuteScalar();
                        if (commandeInsere.quantite > stock)
                        {
                            return BadRequest($"Stock insuffisant. Stock disponible : {stock}");
                        }
                    }

                    string getPrixQuery = "SELECT prix FROM Livre WHERE id = @LivreId";
                    using (SqlCommand getPrixCmd = new SqlCommand(getPrixQuery, connection))
                    {
                        getPrixCmd.Parameters.AddWithValue("@LivreId", idLivre);
                        double prix = (double)getPrixCmd.ExecuteScalar();
                        commandeInsere.total = prix * commandeInsere.quantite;
                    }

                    string upddateQuery = @"UPDATE Commande SET etat = @etat, quantite = @quantite, total = @total, dateCommande = @dateCommande  
                                           WHERE id = @idCommande;";

                    using (SqlCommand cmd = new SqlCommand(upddateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@quantite", commandeInsere.quantite);
                        cmd.Parameters.AddWithValue("@total", commandeInsere.total);
                        cmd.Parameters.AddWithValue("@etat", 1); // État final : validé
                        cmd.Parameters.AddWithValue("@idCommande", commandeInsere.id);
                        cmd.Parameters.AddWithValue("@dateCommande", commandeInsere.dateCommande);
                        cmd.ExecuteNonQuery();

                        commandeInsere.etat = 1;
                    }

                    string updateStockQuery = "UPDATE Stock SET quantite = quantite - @quantite WHERE LivreId = @LivreId";
                    using (SqlCommand updateStockCmd = new SqlCommand(updateStockQuery, connection))
                    {
                        updateStockCmd.Parameters.AddWithValue("@quantite", commandeInsere.quantite);
                        updateStockCmd.Parameters.AddWithValue("@LivreId", idLivre);
                        updateStockCmd.ExecuteNonQuery();
                    }

                }
                return Ok(new { commande = commandeInsere });
            }
            return BadRequest("Invalid Commande request");
        }

        [HttpGet("panier")]
        public IActionResult GetCommandesEnPanier(
            string? recherche = null,
            int pageNumber = 1,
            int pageSize = 10
        )
        {

            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.PrimarySid));

            if (string.IsNullOrEmpty(userId.ToString()))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            List<Commande> commandes = new List<Commande>();
            int totalCount = 0;

            string connectionString = _configuration.GetConnectionString("BibthequeContext");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                StringBuilder countSqlBuilder = new StringBuilder(@"
                    SELECT COUNT(*) 
                    FROM Commande c
                    JOIN Livre l ON c.LivreId = l.id
                    JOIN Utilisateur u ON c.UtilisateurId = u.id
                    WHERE c.etat = 0 and c.UtilisateurId = @utilisateurId");

                if (!string.IsNullOrEmpty(recherche))
                {
                    countSqlBuilder.Append(@"
                    AND (
                        l.titre LIKE @Recherche OR 
                        l.auteur LIKE @Recherche OR 
                        u.nom LIKE @Recherche OR 
                        u.prenom LIKE @Recherche OR 
                        CAST(c.dateCommande AS NVARCHAR) LIKE @Recherche OR
                        CAST(l.prix AS NVARCHAR) LIKE @Recherche OR 
                        CAST(l.nbPage AS NVARCHAR) LIKE @Recherche
                    )");
                }

                using (SqlCommand countCmd = new SqlCommand(countSqlBuilder.ToString(), connection))
                {
                    countCmd.Parameters.AddWithValue("@utilisateurId", userId);
                    if (!string.IsNullOrEmpty(recherche))
                    {
                        countCmd.Parameters.AddWithValue("@Recherche", $"%{recherche}%");
                    }
                    totalCount = (int)countCmd.ExecuteScalar();
                }

                // Requête principale avec pagination
                StringBuilder sqlBuilder = new StringBuilder(@"
                    SELECT * FROM 
                    (
                        SELECT ROW_NUMBER() OVER(ORDER BY c.id) AS RowNum, 
                               c.id, c.quantite, c.total, c.etat, c.dateCommande,
                               l.id AS LivreId, l.titre, l.auteur, l.resume, l.image, l.nbPage, l.prix, l.dateEdition,
                                cat.id CategorieId, cat.nom categorieNom,
                               u.id AS UtilisateurId, u.nom, u.prenom, u.numero, u.adresse
                        FROM Commande c
                        JOIN Livre l ON c.LivreId = l.id
                        JOIN Categorie cat on l.CategorieId = cat.id
                        JOIN Utilisateur u ON c.UtilisateurId = u.id
                        WHERE c.etat = 0 and c.UtilisateurId = @utilisateurId");

                if (!string.IsNullOrEmpty(recherche))
                {
                    sqlBuilder.Append(@"
                        AND (
                            l.titre LIKE @Recherche OR 
                            l.auteur LIKE @Recherche OR 
                            u.nom LIKE @Recherche OR 
                            u.prenom LIKE @Recherche OR 
                            CAST(c.dateCommande AS NVARCHAR) LIKE @Recherche OR
                            CAST(l.prix AS NVARCHAR) LIKE @Recherche OR 
                            CAST(l.nbPage AS NVARCHAR) LIKE @Recherche
                        )");
                }

                sqlBuilder.Append(@") AS RowConstrainedResult
                    WHERE RowNum >= @RowStart AND RowNum < @RowEnd
                    ORDER BY RowNum");

                using (SqlCommand command = new SqlCommand(sqlBuilder.ToString(), connection))
                {
                    int rowStart = (pageNumber - 1) * pageSize + 1;
                    int rowEnd = pageNumber * pageSize + 1;
                    command.Parameters.AddWithValue("@RowStart", rowStart);
                    command.Parameters.AddWithValue("@RowEnd", rowEnd);
                    command.Parameters.AddWithValue("@utilisateurId", userId);
                    if (!string.IsNullOrEmpty(recherche))
                    {
                        command.Parameters.AddWithValue("@Recherche", $"%{recherche}%");
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Commande commande = new Commande
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                quantite = reader.GetInt32(reader.GetOrdinal("quantite")),
                                total = reader.GetDouble(reader.GetOrdinal("total")),
                                etat = reader.GetInt32(reader.GetOrdinal("etat")),
                                dateCommande = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("dateCommande"))),
                                LivreId = reader.GetInt32(reader.GetOrdinal("LivreId")),
                                UtilisateurId = reader.GetInt32(reader.GetOrdinal("UtilisateurId")),
                                Livre = new Livre
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("LivreId")),
                                    titre = reader.GetString(reader.GetOrdinal("titre")),
                                    auteur = reader.GetString(reader.GetOrdinal("auteur")),
                                    resume = reader.GetString(reader.GetOrdinal("resume")),
                                    image = reader.GetString(reader.GetOrdinal("image")),
                                    nbPage = reader.GetInt32(reader.GetOrdinal("nbPage")),
                                    prix = reader.GetDouble(reader.GetOrdinal("prix")),
                                    dateEdition = reader.GetDateTime(reader.GetOrdinal("dateEdition")),
                                    CategorieId = reader.GetInt32(reader.GetOrdinal("CategorieId")),
                                    Categorie = new Categorie
                                    {
                                        id = reader.GetInt32(reader.GetOrdinal("CategorieId")),
                                        nom = reader.GetString(reader.GetOrdinal("CategorieNom"))
                                    }
                                },
                                utilisateur = new Utilisateur
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("UtilisateurId")),
                                    nom = reader.GetString(reader.GetOrdinal("nom")),
                                    prenom = reader.GetString(reader.GetOrdinal("prenom")),
                                    adresse = reader.GetString(reader.GetOrdinal("adresse")),
                                    numero = reader.GetString(reader.GetOrdinal("numero")),
                                }
                            };

                            commandes.Add(commande);
                        }
                    }
                }
            }

            var result = new
            {
                TotalCount = totalCount,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Commandes = commandes
            };

            return Ok(result);
        }

        [HttpGet("valide")]
        public IActionResult GetCommandesValide(
            string? recherche = null,
            int pageNumber = 1,
            int pageSize = 10
        )
        {

            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.PrimarySid));

            if (string.IsNullOrEmpty(userId.ToString()))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            List<Commande> commandes = new List<Commande>();
            int totalCount = 0;

            string connectionString = _configuration.GetConnectionString("BibthequeContext");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                StringBuilder countSqlBuilder = new StringBuilder(@"
                    SELECT COUNT(*) 
                    FROM Commande c
                    JOIN Livre l ON c.LivreId = l.id
                    JOIN Utilisateur u ON c.UtilisateurId = u.id
                    WHERE c.etat = 1 and c.UtilisateurId = @utilisateurId");

                if (!string.IsNullOrEmpty(recherche))
                {
                    countSqlBuilder.Append(@"
                    AND (
                        l.titre LIKE @Recherche OR 
                        l.auteur LIKE @Recherche OR 
                        u.nom LIKE @Recherche OR 
                        u.prenom LIKE @Recherche OR 
                        CAST(c.dateCommande AS NVARCHAR) LIKE @Recherche OR
                        CAST(l.prix AS NVARCHAR) LIKE @Recherche OR 
                        CAST(l.nbPage AS NVARCHAR) LIKE @Recherche
                    )");
                }

                using (SqlCommand countCmd = new SqlCommand(countSqlBuilder.ToString(), connection))
                {
                    countCmd.Parameters.AddWithValue("@utilisateurId", userId);
                    if (!string.IsNullOrEmpty(recherche))
                    {
                        countCmd.Parameters.AddWithValue("@Recherche", $"%{recherche}%");
                    }
                    totalCount = (int)countCmd.ExecuteScalar();
                }

                // Requête principale avec pagination
                StringBuilder sqlBuilder = new StringBuilder(@"
                    SELECT * FROM 
                    (
                        SELECT ROW_NUMBER() OVER(ORDER BY c.id) AS RowNum, 
                               c.id, c.quantite, c.total, c.etat, c.dateCommande,
                               l.id AS LivreId, l.titre, l.auteur, l.resume, l.image, l.nbPage, l.prix, l.dateEdition,
                                cat.id CategorieId, cat.nom categorieNom,
                               u.id AS UtilisateurId, u.nom, u.prenom, u.numero, u.adresse
                        FROM Commande c
                        JOIN Livre l ON c.LivreId = l.id
                        JOIN Categorie cat on l.CategorieId = cat.id
                        JOIN Utilisateur u ON c.UtilisateurId = u.id
                        WHERE c.etat = 1 and c.UtilisateurId = @utilisateurId");

                if (!string.IsNullOrEmpty(recherche))
                {
                    sqlBuilder.Append(@"
                        AND (
                            l.titre LIKE @Recherche OR 
                            l.auteur LIKE @Recherche OR 
                            u.nom LIKE @Recherche OR 
                            u.prenom LIKE @Recherche OR 
                            CAST(c.dateCommande AS NVARCHAR) LIKE @Recherche OR
                            CAST(l.prix AS NVARCHAR) LIKE @Recherche OR 
                            CAST(l.nbPage AS NVARCHAR) LIKE @Recherche
                        )");
                }

                sqlBuilder.Append(@") AS RowConstrainedResult
                    WHERE RowNum >= @RowStart AND RowNum < @RowEnd
                    ORDER BY RowNum");

                using (SqlCommand command = new SqlCommand(sqlBuilder.ToString(), connection))
                {
                    int rowStart = (pageNumber - 1) * pageSize + 1;
                    int rowEnd = pageNumber * pageSize + 1;
                    command.Parameters.AddWithValue("@RowStart", rowStart);
                    command.Parameters.AddWithValue("@RowEnd", rowEnd);
                    command.Parameters.AddWithValue("@utilisateurId", userId);
                    if (!string.IsNullOrEmpty(recherche))
                    {
                        command.Parameters.AddWithValue("@Recherche", $"%{recherche}%");
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Commande commande = new Commande
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                quantite = reader.GetInt32(reader.GetOrdinal("quantite")),
                                total = reader.GetDouble(reader.GetOrdinal("total")),
                                etat = reader.GetInt32(reader.GetOrdinal("etat")),
                                dateCommande = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("dateCommande"))),
                                LivreId = reader.GetInt32(reader.GetOrdinal("LivreId")),
                                UtilisateurId = reader.GetInt32(reader.GetOrdinal("UtilisateurId")),
                                Livre = new Livre
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("LivreId")),
                                    titre = reader.GetString(reader.GetOrdinal("titre")),
                                    auteur = reader.GetString(reader.GetOrdinal("auteur")),
                                    resume = reader.GetString(reader.GetOrdinal("resume")),
                                    image = reader.GetString(reader.GetOrdinal("image")),
                                    nbPage = reader.GetInt32(reader.GetOrdinal("nbPage")),
                                    prix = reader.GetDouble(reader.GetOrdinal("prix")),
                                    dateEdition = reader.GetDateTime(reader.GetOrdinal("dateEdition")),
                                    CategorieId = reader.GetInt32(reader.GetOrdinal("CategorieId")),
                                    Categorie = new Categorie
                                    {
                                        id = reader.GetInt32(reader.GetOrdinal("CategorieId")),
                                        nom = reader.GetString(reader.GetOrdinal("CategorieNom"))
                                    }
                                },
                                utilisateur = new Utilisateur
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("UtilisateurId")),
                                    nom = reader.GetString(reader.GetOrdinal("nom")),
                                    prenom = reader.GetString(reader.GetOrdinal("prenom")),
                                    adresse = reader.GetString(reader.GetOrdinal("adresse")),
                                    numero = reader.GetString(reader.GetOrdinal("numero")),
                                }
                            };

                            commandes.Add(commande);
                        }
                    }
                }
            }

            var result = new
            {
                TotalCount = totalCount,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Commandes = commandes
            };

            return Ok(result);
        }

        [HttpDelete("annule/{idCommande}")]
        public IActionResult AnnulerCommande(int idCommande)
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.PrimarySid));

            if (string.IsNullOrEmpty(userId.ToString()))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            string connectionString = _configuration.GetConnectionString("BibthequeContext");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string getUserCommandeQuery = "SELECT UtilisateurId FROM Commande WHERE id = @commandeId";
                using (SqlCommand userCommandeCmd = new SqlCommand(getUserCommandeQuery, connection))
                {
                    userCommandeCmd.Parameters.AddWithValue("@commandeId", idCommande);
                    int user = (int)userCommandeCmd.ExecuteScalar();
                    if (user != userId)
                    {
                        return BadRequest("Ce n'est pas votre commande");
                    }
                }

                string geEtatCommandeQuery = "SELECT etat FROM Commande WHERE id = @commandeId";
                using (SqlCommand etatCommandeCmd = new SqlCommand(geEtatCommandeQuery, connection))
                {
                    etatCommandeCmd.Parameters.AddWithValue("@commandeId", idCommande);
                    int etat = (int)etatCommandeCmd.ExecuteScalar();
                    if(etat == 1)
                    {
                        return BadRequest("Commande déjà validé , ne peut pas être supprimé");
                    }
                }

                string deleteCommandeQuery = "DELETE FROM Commande WHERE id = @commandeId";
                using (SqlCommand deleteCommandeCmd = new SqlCommand(deleteCommandeQuery, connection))
                {
                    deleteCommandeCmd.Parameters.AddWithValue("@commandeId", idCommande);
                    deleteCommandeCmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Commande annulé et supprimé" });
        }
    }
}
