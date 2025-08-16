using ConstructionApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using ConstructionApp.Validators;

namespace ConstructionApp.ViewModels
{
    public class WorkSiteViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Address { get; set; } = string.Empty;

        public IFormFile? Picture { get; set; } 
        public string? PicturePath { get; set; }
        public string? CAP { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public float Amount { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public float Safety { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime DateStart { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [DateGreaterThan("DateStart", ErrorMessage = "End date must be after start date.")]
        public DateTime DateEnd { get; set; }

        [Display(Name = "Construction Manager")]
        [Required]
        public int ManagerId { get; set; }

        public SelectList? ManagerList { get; set; }

        public List<int> SelectedWorkerIds { get; set; } = new();

        public List<User>? AssignedWorkers { get; set; }

        public List<AttachFileViewModel> AttachFiles { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateEnd < DateStart)
            {
                yield return new ValidationResult(
                    "End date cannot be earlier than start date.",
                    new[] { nameof(DateEnd) }
                );
            }
        }
    }
}