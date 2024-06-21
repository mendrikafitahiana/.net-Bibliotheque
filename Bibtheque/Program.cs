using Bibtheque.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Bibliotheque.Models.Context;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Configuration du contexte de base de donn�es
builder.Services.AddDbContext<BibthequeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BibthequeContext") ?? throw new InvalidOperationException("Connection string 'BibthequeContext' not found")));

// Configuration de l'authentification
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Utilisateur/Login";
        options.LogoutPath = "/Utilisateur/Logout";
        options.AccessDeniedPath = "/Utilisateur/AccessDenied";
    });

// Ajout des services au conteneur
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuration du pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Utilisateur}/{action=Login}/{id?}");

app.Run();
