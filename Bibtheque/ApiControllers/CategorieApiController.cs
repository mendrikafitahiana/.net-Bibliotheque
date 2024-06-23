using Bibtheque.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bibtheque.ApiControllers
{
    [Route("api/categorie")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CategorieApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CategorieApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("categories")]
        public IActionResult GetCategorie() 
        {
            List<Categorie> categories = new List<Categorie>();

            string connectionString = _configuration.GetConnectionString("BibthequeContext");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string categQuery = "SELECT * FROM Categorie";
                using (SqlCommand categCmd = new SqlCommand(categQuery, connection))
                {
                    using (SqlDataReader reader = categCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categorie categorie = new Categorie
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                nom = reader.GetString(reader.GetOrdinal("nom"))
                            };

                            categories.Add(categorie);
                        }
                    }
                }
            }
            return Ok(categories);
        }
    }
}
