using Microsoft.AspNetCore.Mvc;
using ConstructionApp.Data;
using ConstructionApp.Models;
using ConstructionApp.Helpers;
using ConstructionApp.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ConstructionApp.Components
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public FooterViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vehicles = await _context.Vehicles
                .Select(v => new
                {
                    v.Id,
                    v.Plate,
                    v.Picture,
                    v.DateInsurance,
                    v.DateRevision,
                    v.DateMaintenance
                })
                .ToListAsync();

            var maintenancedata = vehicles
                .SelectMany(v => new[]
                {
                    new AttachFileViewModel {
                        Id = v.Id, Type = "Maintenance", Name = v.Plate, Renewed = false, Dumped = false, FileName = "Picture",
                        StoredFilePath = !string.IsNullOrEmpty(v.Picture) ? v.Picture : "\\images\\vehicle.svg",
                        Deadline = v.DateInsurance, Description = "Insurance"
                    },
                    new AttachFileViewModel {
                        Id = v.Id, Type = "Maintenance", Name = v.Plate, Renewed = false, Dumped = false, FileName = "Picture",
                        StoredFilePath = !string.IsNullOrEmpty(v.Picture) ? v.Picture : "\\images\\vehicle.svg",
                        Deadline = v.DateRevision, Description = "Revision"
                    },
                    new AttachFileViewModel {
                        Id = v.Id, Type = "Maintenance", Name = v.Plate, Renewed = false, Dumped = false, FileName = "Picture",
                        StoredFilePath = !string.IsNullOrEmpty(v.Picture) ? v.Picture : "\\images\\vehicle.svg",
                        Deadline = v.DateMaintenance, Description = "Maintenance"
                    }
                }).ToList();

            var attachdata = await _context.AttachFiles
                .Where(x => !x.Renewed)
                .Select(x => new AttachFileViewModel
                {
                    Deadline = x.Deadline,
                    Description = x.Description,
                    FileName = x.FileName,
                    StoredFilePath = x.StoredFilePath,
                    Type = x.WorkSite != null ? "WorkSite" :
                           x.User != null ? "Employee" :
                           x.Vehicle != null ? "Vehicle" : "",
                    Name = x.WorkSite != null ? x.WorkSite.Name :
                           x.User != null ? x.User.UserName :
                           x.Vehicle != null ? x.Vehicle.Plate : ""
                })
                .ToListAsync();

            var now = DateTime.Now;
            var data = attachdata.Concat(maintenancedata)
                .Where(x => x.Deadline > now && x.Deadline < now.AddDays(7))
                .ToList();

            var typeList = data
                .GroupBy(x => x.Type)
                .Select(g => new SelectListItem
                {
                    Text = $"{(g.Key)} ({g.Count()})",
                    Value = g.Key,
                })
                .ToList();

            AlarmViewModel model = new AlarmViewModel
            {
                totalCount = data.Count(),
                AlarmHeader = typeList
            };

            return View(model);
        }
    }
}
