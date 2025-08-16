using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConstructionApp.ViewModels
{
    public class AlarmViewModel
    {
        public int totalCount {  get; set; }
        public List<SelectListItem> AlarmHeader { get; set; } = new();
    }
}
