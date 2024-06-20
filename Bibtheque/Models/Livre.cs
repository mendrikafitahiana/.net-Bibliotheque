using System.ComponentModel.DataAnnotations;

namespace Bibtheque.Models
{
    public class Livre
    {
        public int id { get; set; }
        public string titre { get; set; }
        public string auteur { get; set; }
        public string resume { get; set; }
        public string image { get; set; }
        public int nbPage { get; set; }
        public double prix { get; set; }

        [DataType(DataType.Date)]
        public DateTime dateEdition { get; set; }
        public int CategorieId { get; set; }
        public Categorie? Categorie { get; set; }
        public Stock? Stock { get; set; }
    }
}
