using Bibtheque.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Bibliotheque.Models.Context;

var builder = WebApplication.CreateBuilder(args);

// Configuration du contexte de base de donn�es
builder.Services.AddDbContext<BibthequeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BibthequeContext") ?? throw new InvalidOperationException("Connection string 'BibthequeContext' not found")));

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
