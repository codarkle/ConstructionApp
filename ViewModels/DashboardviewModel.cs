using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConstructionApp.ViewModels
{
    public class DashboardViewModel
    {
        public List<WorkSiteCardViewModel> WorkSiteCards { get; set; } = new();
        public List<SelectListItem> DeadlineList { get; set; } = new();
    }

    public class WorkSiteCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Picture {  get; set; }
        public string Address { get; set; } = string.Empty;
        public float Amount { get; set; }
        public float Safety { get; set; }
        public float MaterialCost { get; set; }
        public float WorkerSalary { get; set; }

        public string ManagerName = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DateStart { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateEnd { get; set; }
    } 
}
