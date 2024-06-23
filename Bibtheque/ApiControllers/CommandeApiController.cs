using Bibtheque.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using NuGet.Common;
using System.Security.Claims;
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
    }
}
