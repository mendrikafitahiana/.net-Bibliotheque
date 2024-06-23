using Bibtheque.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Text.Json;

namespace Bibtheque.ApiControllers
{
    [Route("api/paiement")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PaiementApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CommandeApiController _commandeController;


        public PaiementApiController(IConfiguration configuration, CommandeApiController commandeController)
        {
            _configuration = configuration;
            _commandeController = commandeController;
        }

        [HttpPost]
        public IActionResult Paiement([FromBody] JsonElement paiement)
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.PrimarySid));

            if (string.IsNullOrEmpty(userId.ToString()))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            if (paiement.TryGetProperty("idCommande", out JsonElement commandeElement) &&
                paiement.TryGetProperty("carte", out JsonElement carteElement) &&
                paiement.TryGetProperty("idRegion", out JsonElement regionElement)
                )
            {
                int idCommande = commandeElement.GetInt32();
                string carte = carteElement.GetString();
                int idRegion = regionElement.GetInt32();

                DateOnly dateComm = new DateOnly();
                int nbJour = 0;
                DateOnly dateLivraison = new DateOnly();

                Paiement paiementInsere = new Paiement();

                string connectionString = _configuration.GetConnectionString("BibthequeContext");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string checkPaiementQuery = "SELECT COUNT(*) FROM Paiement WHERE CommandeId = @commandeId";
                            using (SqlCommand checkPaiementCmd = new SqlCommand(checkPaiementQuery, connection, transaction))
                            {
                                checkPaiementCmd.Parameters.AddWithValue("@commandeId", idCommande);
                                int commandeCount = (int)checkPaiementCmd.ExecuteScalar();
                                if (commandeCount > 0)
                                {
                                    return Ok(new
                                    {
                                        message = "Paiement déjà effectué"
                                    });
                                }
                            }

                            string checkCommandeQuery = "SELECT COUNT(*) FROM Commande WHERE id = @commandeId";
                            using (SqlCommand checkCommandeCmd = new SqlCommand(checkCommandeQuery, connection, transaction))
                            {
                                checkCommandeCmd.Parameters.AddWithValue("@commandeId", idCommande);
                                int commandeCount = (int)checkCommandeCmd.ExecuteScalar();
                                if (commandeCount == 0)
                                {
                                    return NotFound("La Commande spécifiée n'existe pas.");
                                }
                            }

                            string getDateCommandeQuery = "SELECT dateCommande FROM Commande WHERE id = @commandeId";
                            using (SqlCommand dateCommandeCmd = new SqlCommand(getDateCommandeQuery, connection, transaction))
                            {
                                dateCommandeCmd.Parameters.AddWithValue("@commandeId", idCommande);
                                DateTime datecommande = (DateTime)dateCommandeCmd.ExecuteScalar();
                                dateComm = DateOnly.FromDateTime(datecommande);
                            }

                            string getNbJourQuery = "SELECT nbJourLivraison FROM Region WHERE id = @regionId";
                            using (SqlCommand nbJourCmd = new SqlCommand(getNbJourQuery, connection, transaction))
                            {
                                nbJourCmd.Parameters.AddWithValue("@regionId", idRegion);
                                int nbj = Convert.ToInt32(nbJourCmd.ExecuteScalar());
                                nbJour = nbj;
                            }

                            Console.WriteLine(nbJour);
                            Console.WriteLine(dateComm);

                            dateLivraison = dateComm.AddDays(nbJour);
                            Console.WriteLine(dateLivraison);


                            paiementInsere.CommandeId = idCommande;
                            paiementInsere.numeroCarte = carte;
                            paiementInsere.RegionId = idRegion;
                            paiementInsere.dateLivraison = dateLivraison;

                            string insertQuery = @"INSERT INTO Paiement (CommandeId, numeroCarte, RegionId, dateLivraison) 
                                               VALUES (@commandeId, @numeroCarte, @regionId, @dateLivraison);
                                               SELECT SCOPE_IDENTITY();";

                            using (SqlCommand cmd = new SqlCommand(insertQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@commandeId", paiementInsere.CommandeId);
                                cmd.Parameters.AddWithValue("@numeroCarte", paiementInsere.numeroCarte);
                                cmd.Parameters.AddWithValue("@regionId", paiementInsere.RegionId);
                                cmd.Parameters.AddWithValue("@dateLivraison", paiementInsere.dateLivraison);

                                paiementInsere.id = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            var resultatValidation = _commandeController.ValiderCommande(paiement, userId);
                            if (resultatValidation is OkObjectResult okResult)
                            {
                                var commandeValidee = okResult.Value;
                                transaction.Commit();

                                return Ok(new
                                {
                                    message = "Paiement effectué et commande validée",
                                    commande = commandeValidee,
                                    paiement = paiementInsere,
                                });
                            }
                            else if (resultatValidation is BadRequestObjectResult badRequestResult)
                            {
                                transaction.Rollback();
                                return BadRequest(badRequestResult.Value);
                            }
                            else if (resultatValidation is NotFoundObjectResult notFoundResult)
                            {
                                transaction.Rollback();
                                return NotFound(notFoundResult.Value);
                            }
                            else
                            {
                                transaction.Rollback();
                                return StatusCode(500, "Une erreur inattendue s'est produite");
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return StatusCode(500, $"Une erreur est survenue : {ex.Message}");
                        }
                    }
                }
            }

            return BadRequest("Invalid Paiement request");
        }
    }
}
