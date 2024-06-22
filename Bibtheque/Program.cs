using Bibtheque.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Bibliotheque.Models.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Bibtheque.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration du contexte de base de donn�es
builder.Services.AddDbContext<BibthequeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BibthequeContext") ?? throw new InvalidOperationException("Connection string 'BibthequeContext' not found")));

// Configuration de l'authentification
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Utilisateur/Login";
    options.LogoutPath = "/Utilisateur/Logout";
    options.AccessDeniedPath = "/Utilisateur/AccessDenied";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Ajout des services au conteneur
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddScoped<IUserService, UserService>();

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

// routage API
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/", async context =>
{
    var userService = context.RequestServices.GetRequiredService<IUserService>();
    if (userService.IsUserLoggedIn(context))
    {
        context.Response.Redirect("/Livre/Index");
    }
    else
    {
        context.Response.Redirect("/Utilisateur/Login");
    }
});

app.Run();
