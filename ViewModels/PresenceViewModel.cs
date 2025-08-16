using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ConstructionApp.ViewModels
{
    public class PresenceEmployeeViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public bool IsPresent { get; set; } = true;
        public float HS { get; set; }
        public float HR { get; set; }
        public float Cost { get; set; }
    }

    public class PresenceDailyViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [Required]
        public int WorkSiteId { get; set; }
        public SelectList? EmployeeList { get; set; }
        public List<PresenceEmployeeViewModel>? Employees { get; set; }
    } 
}
