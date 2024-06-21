using Bibtheque.Models.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Bibtheque.Controllers
{
    public class UtilisateurController : Controller
    {
        private readonly BibthequeContext _context;

        public UtilisateurController(BibthequeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var utilisateur = await _context.Utilisateur
                .FirstOrDefaultAsync(u => u.username == username);

            if (utilisateur == null || !VerifyPassword(password, utilisateur.password))
            {
                ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe incorrect.");
                return View();
            }

            if(utilisateur != null && utilisateur.role != 1)
            {
                ModelState.AddModelError(string.Empty, "Pas d'accès à l'application! Accès administrateur.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, utilisateur.username),
                new Claim(ClaimTypes.Surname, utilisateur.nom),
                new Claim(ClaimTypes.Role, utilisateur.role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, utilisateur.id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Livre");
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            return enteredPassword == storedPassword;
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Utilisateur");
        }
    }
}
