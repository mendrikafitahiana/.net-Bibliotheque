using Bibtheque.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Bibtheque.ApiControllers
{
    [Route("api/region")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RegionApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RegionApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetRegion()
        {
            List<Region> regions = new List<Region>();

            string connectionString = _configuration.GetConnectionString("BibthequeContext");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string regionQuery = "SELECT * FROM Region";
                using (SqlCommand regionCmd = new SqlCommand(regionQuery, connection))
                {
                    using (SqlDataReader reader = regionCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Region region = new Region
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                nom = reader.GetString(reader.GetOrdinal("nom")),
                                nbJourLivraison = reader.GetString(reader.GetOrdinal("nbJourLivraison"))
                            };

                            regions.Add(region);
                        }
                    }
                }
            }
            return Ok(regions);
        }
    }
}
