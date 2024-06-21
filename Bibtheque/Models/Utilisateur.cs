namespace Bibtheque.Models
{
    public class Utilisateur
    {
        public int id { get; set; }
        public string nom { get; set; }
        public string prenom { get; set; }
        public string numero { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string adresse { get; set; }
        public int role { get; set; } //role 1 : admin, role 2 : client
    }
}
