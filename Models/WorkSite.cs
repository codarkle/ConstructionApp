using System.ComponentModel.DataAnnotations.Schema;

namespace ConstructionApp.Models
{
    public class WorkSite
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? CAP { get; set; }

        public float Amount { get; set; } 
        public float Safety { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string? Picture { get; set; }

        //foreign key
        public int ManagerId { get; set; }
        public User Manager { get; set; } = null!;
        public ICollection<User> Workers { get; set; } = new List<User>();
        public ICollection<AttachFile> AttachFiles { get; set; } = new List<AttachFile>();
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<Presence> Presences { get; set; } = new List<Presence>(); 
    }
}