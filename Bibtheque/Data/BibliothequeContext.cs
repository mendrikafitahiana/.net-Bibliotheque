using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Bibtheque.Models;

namespace Bibliotheque.Models.Context
{
    public class BibliothequeContext : DbContext
    {
        public BibliothequeContext (DbContextOptions<BibliothequeContext> options)
            : base(options)
        {
        }

        public DbSet<Bibtheque.Models.Categorie> Categorie { get; set; } = default!;
    }
}
