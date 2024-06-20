namespace Bibtheque.Models
{
    public class Categorie
    {
        public Categorie()
        {
            Livres = new List<Livre>();
        }
        public int id { get; set; }
        public string nom { get; set; }
        public ICollection<Livre> Livres { get; set; }
    }
}
