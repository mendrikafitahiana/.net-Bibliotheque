using System.ComponentModel.DataAnnotations;

namespace Bibtheque.Models
{
    public class Commande
    {
        public int id { get; set; }
        public int quantite { get; set; }
        public double total { get; set; }
        public int LivreId { get; set; }
        public Livre? Livre { get; set; }
        public int etat { get; set; } //etat 0 panier, etat 1 commande déjà acheté
        public int UtilisateurId { get; set; }
        public Utilisateur? utilisateur { get; set; }

        [DataType(DataType.Date)]
        public DateOnly dateCommande { get; set; }

    }
}
