using Microsoft.EntityFrameworkCore;

namespace Bibtheque.Models.Context
{
    public class BibthequeContext : DbContext
    {
        public BibthequeContext(DbContextOptions<BibthequeContext> options)
            : base(options) { }

        public DbSet<Bibtheque.Models.Categorie> Categorie { get; set; } = default!;
        public DbSet<Bibtheque.Models.Livre> Livre { get; set; } = default!;
        public DbSet<Bibtheque.Models.Stock> Stock { get; set; } = default!;
        public DbSet<Utilisateur> Utilisateur { get; set; } = default!;
        public DbSet<Region> Region { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Livre>()
             .HasOne(l => l.Categorie)
             .WithMany(c => c.Livres)
             .HasForeignKey(l => l.CategorieId)
             .IsRequired();

            modelBuilder.Entity<Stock>()
            .HasOne(s => s.Livre)
            .WithOne(l => l.Stock)
            .HasForeignKey<Stock>(s => s.LivreId)
            .IsRequired(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
