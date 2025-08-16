using System.ComponentModel.DataAnnotations.Schema;

namespace ConstructionApp.Models
{
    public class Purchase
    {
        public int Id { get; set; }
         
        public float Quantity { get; set; }     
         
        public float Amount { get; set; }

        public DateTime DateDoc { get; set; }
        public string DocNumber { get; set; } = string.Empty;

        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public int SupplierId { get; set; }
        public User Supplier { get; set; } = null!;

        public int WorkSiteId { get; set; }
        public WorkSite WorkSite { get; set; } = null!;
    }

}
