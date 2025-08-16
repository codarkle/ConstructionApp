using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ConstructionApp.ViewModels
{
    public class PurchaseViewModel
    {
        public int Id { get; set; }

        [Required]
        public string DocNumber { get; set; } = string.Empty;

        [Required]
        public DateTime DateDoc { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [Required]
        public int WorkSiteId { get; set; }

        [Required]
        public float Quantity { get; set; }

        [Required]
        public float Amount { get; set; }

        public SelectList? Materials { get; set; }
        public SelectList? Suppliers { get; set; }
        public SelectList? WorkSites { get; set; }
    }
}
