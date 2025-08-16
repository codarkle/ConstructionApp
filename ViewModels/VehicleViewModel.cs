using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ConstructionApp.ViewModels
{
    public class VehicleViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Description { get; set; } = null!;
        public IFormFile? Picture { get; set; }
        public string? PicturePath { get; set; }
        [Required]
        public string Plate { get; set; } = null!;
        [Required]
        public DateTime DateInsurance { get; set; }
        [Required]
        public DateTime DateRevision { get; set; }
        [Required]
        public DateTime DateMaintenance { get; set; }
        public List<AttachFileViewModel> AttachFiles { get; set; } = new ();
    } 
}
