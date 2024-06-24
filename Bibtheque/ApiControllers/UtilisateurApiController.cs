using Bibtheque.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Bibtheque.ApiControllers
{
    [Route("api/utilisateur")]
    [ApiController]
    public class UtilisateurApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UtilisateurApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] JsonElement utilisateur)
        {
            if (utilisateur.TryGetProperty("username", out JsonElement usernameElement) &&
                utilisateur.TryGetProperty("password", out JsonElement passwordElement))
            {
                string username = usernameElement.GetString();
                string password = passwordElement.GetString();

                var user = IsValidUser(username, password);
                if (user != null)
                {
                    var token = GenerateJwtToken(user);
                    return Ok(new { token });
                }
                return Unauthorized();
            }
            return BadRequest("Invalid login request");
        }

        private Utilisateur IsValidUser(string username, string password)
        {
            string connectionString = _configuration.GetConnectionString("BibthequeContext");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Utilisateur WHERE Username = @Username";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string stored = reader["password"].ToString();

                            if (VerifyPassword(password, stored))
                            {
                                return new Utilisateur
                                {
                                    id = Convert.ToInt32(reader["id"]),
                                    username = reader["username"].ToString(),
                                    nom = reader["nom"].ToString(),
                                    prenom = reader["prenom"].ToString(),
                                    role = Convert.ToInt32(reader["role"]),
                                    numero = reader["numero"].ToString(),
                                    adresse = reader["adresse"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            return null; 
        }

        private string GenerateJwtToken(Utilisateur user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.username),
                new Claim(ClaimTypes.PrimarySid, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.nom),
                new Claim(ClaimTypes.Email, user.numero),
                new Claim(ClaimTypes.Surname, user.prenom),
                new Claim(ClaimTypes.Role, user.role.ToString()),
                new Claim(ClaimTypes.Locality, user.adresse)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(500),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            return enteredPassword == storedPassword;
        }
    }
}
