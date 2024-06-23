using System.ComponentModel.DataAnnotations;

namespace Bibtheque.Models
{
    public class Paiement
    {
        public int id { get; set; }
        public int CommandeId { get; set; }
        public Commande? commande { get; set; }
        public string numeroCarte { get; set; }
        public int RegionId { get; set; }
        public Region? region { get; set; }

        [DataType(DataType.Date)]
        public DateOnly dateLivraison { get; set; }
    }
}
