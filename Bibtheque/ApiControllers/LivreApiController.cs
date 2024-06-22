using Bibtheque.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Bibtheque.ApiControllers
{
    [Route("api/livre")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LivreApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LivreApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("livres")]
        public IActionResult GetLivres(int pageNumber = 1, int pageSize = 10)
        {
            List<Livre> livres = new List<Livre>();

            string connectionString = _configuration.GetConnectionString("BibthequeContext");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT * FROM 
                    (
                        SELECT ROW_NUMBER() OVER(ORDER BY Livre.Id) AS RowNum, 
                               Livre.id, Livre.titre, Livre.auteur, Livre.resume, Livre.image, Livre.nbPage, 
                               Livre.prix, Livre.dateEdition, 
                               Categorie.id AS categorieId, Categorie.nom AS categorieNom,
                               Stock.id AS stockId, Stock.quantite AS exemplaire
                        FROM Livre
                        JOIN Categorie ON Categorie.id = Livre.CategorieId
                        JOIN Stock ON Livre.id = Stock.LivreId
                        WHERE Stock.quantite > 0
                    ) AS RowConstrainedResult
                    WHERE RowNum >= @RowStart AND RowNum < @RowEnd
                    ORDER BY RowNum";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    int rowStart = (pageNumber - 1) * pageSize + 1;
                    int rowEnd = pageNumber * pageSize + 1;

                    command.Parameters.AddWithValue("@RowStart", rowStart);
                    command.Parameters.AddWithValue("@RowEnd", rowEnd);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            livres.Add(new Livre
                            {
                                id = Convert.ToInt32(reader["id"]),
                                titre = reader["titre"].ToString(),
                                auteur = reader["auteur"].ToString(),
                                resume = reader["resume"].ToString(),
                                image = reader["image"].ToString(),
                                nbPage = Convert.ToInt32(reader["nbPage"]),
                                prix = Convert.ToDouble(reader["prix"]),
                                dateEdition = Convert.ToDateTime(reader["dateEdition"]),
                                CategorieId = Convert.ToInt32(reader["categorieId"]),
                                Categorie = new Categorie
                                {
                                    id = Convert.ToInt32(reader["categorieId"]),
                                    nom = reader["categorieNom"].ToString()
                                },
                                Stock = new Stock
                                {
                                    id = Convert.ToInt32(reader["stockId"]),
                                    quantite = Convert.ToInt32(reader["exemplaire"]),
                                    LivreId = Convert.ToInt32(reader["id"])
                                }
                            });
                        }
                    }
                }
            }

            return Ok(new { PageNumber = pageNumber, PageSize = pageSize, Livres = livres });
        }
    }
}
