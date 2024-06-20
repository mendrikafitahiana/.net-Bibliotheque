namespace Bibtheque.Models
{
    public class Stock
    {
        public int id { get; set; }
        public int quantite { get; set; }
        public int LivreId { get; set; }
        public Livre? Livre { get; set; }
    }
}
