using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConstructionApp.ViewModels
{
    public class MaintenanceViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Vehicle")]
        [Required]
        public int VehicleId { get; set; }

        [Display(Name = "Date Out")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOut { get; set; }

        [Display(Name = "KM Out")]
        [Required]
        public int KmOut { get; set; }

        [Display(Name = "Date In")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateIn { get; set; }

        [Display(Name = "KM In")]
        [Required]
        public int KmIn { get; set; }

        [Display(Name = "Driver")]
        [Required]
        public string Driver { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [Required]
        public string Description { get; set; } = string.Empty;

        // Dropdown list for vehicles
        public IEnumerable<SelectListItem>? VehicleList { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateIn < DateOut)
            {
                yield return new ValidationResult(
                    "DateIn cannot be earlier than DateOut.",
                    new[] { nameof(KmIn) }
                );
            }
            if (KmIn < KmOut)
            {
                yield return new ValidationResult(
                    "KmIn cannot be smaller than KmOut.",
                    new[] { nameof(DateIn) }
                );
            }
        }
    }
}
