using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConstructionApp.Data;
using ConstructionApp.Models;
using ConstructionApp.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ConstructionApp.Controllers
{
    public class MaintenancesController : AppController
    {
        public MaintenancesController(UserManager<User> userManager, AppDbContext context)
            : base(userManager, context)
        {
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        public IActionResult Index()
        { 
            return View();
        }

        // GET: /Maintenances/Create (Modal)
        [Authorize(Roles = "Admin,Surveyor,Manager")]
        public IActionResult Create()
        {
            var vm = new MaintenanceViewModel
            {
                VehicleList = _context.Vehicles
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.Description} ({v.Plate})"
                    }).ToList(),
                DateOut = DateTime.Today,
                DateIn = DateTime.Today.AddDays(1)
            };

            return PartialView("_Create", vm);
        }

        // POST: /Maintenances/Create 
        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MaintenanceViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.VehicleList = _context.Vehicles.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.Description} ({v.Plate})"
                }).ToList();

                return PartialView("_Create", vm);
            }

            var entity = new Maintenance
            {
                VehicleId = vm.VehicleId,
                DateOut = vm.DateOut,
                KmOut = vm.KmOut,
                DateIn = vm.DateIn,
                KmIn = vm.KmIn,
                Driver = vm.Driver,
                Description = vm.Description
            };

            _context.Maintenances.Add(entity);
            _context.SaveChanges();
            return RedirectToAction("Index", "Maintenances");
        }

        // GET: /Maintenances/Edit/{id}
        [Authorize(Roles = "Admin,Surveyor,Manager")]
        public IActionResult Edit(int id)
        {
            var entity = _context.Maintenances.Find(id);
            if (entity == null)
                return NotFound();

            var vm = new MaintenanceViewModel
            {
                Id = entity.Id,
                VehicleId = entity.VehicleId,
                DateOut = entity.DateOut,
                KmOut = entity.KmOut,
                DateIn = entity.DateIn,
                KmIn = entity.KmIn,
                Driver = entity.Driver,
                Description = entity.Description,
                VehicleList = _context.Vehicles.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.Description} ({v.Plate})"
                }).ToList()
            };

            return PartialView("_Edit", vm);
        }

        // POST: /Maintenances/Edit
        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MaintenanceViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.VehicleList = _context.Vehicles.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.Description} ({v.Plate})"
                }).ToList();

                return PartialView("_Edit", vm);
            }

            var entity = _context.Maintenances.Find(vm.Id);
            if (entity == null)
                return NotFound();

            entity.VehicleId = vm.VehicleId;
            entity.DateOut = vm.DateOut;
            entity.KmOut = vm.KmOut;
            entity.DateIn = vm.DateIn;
            entity.KmIn = vm.KmIn;
            entity.Driver = vm.Driver;
            entity.Description = vm.Description;

            _context.SaveChanges();
            return RedirectToAction("Index", "Maintenances");
        }

        // POST: /Maintenances/Delete/{id}
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var entity = _context.Maintenances.Find(id);
            if (entity == null)
                return NotFound();

            _context.Maintenances.Remove(entity);
            _context.SaveChanges();
            return RedirectToAction("Index", "Maintenances");
        }

        // POST: /Maintenances/GetMaintenanceList (AJAX for DataTables)
        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        public JsonResult GetMaintenanceList()
        {
            int totalRecord = 0;
            int filterRecord = 0;

            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            var query = _context.Maintenances
                .Include(m => m.Vehicle)
                .AsQueryable();

            totalRecord = query.Count();
             
            if (!string.IsNullOrEmpty(searchValue))
            {
                var lowerSearch = searchValue.ToLower();

                query = query.Where(m =>
                    (m.Vehicle.Description != null && m.Vehicle.Description.ToLower().Contains(lowerSearch)) ||
                    (m.Vehicle.Plate != null && m.Vehicle.Plate.ToLower().Contains(lowerSearch)) ||
                    (m.Driver != null && m.Driver.ToLower().Contains(lowerSearch)) ||
                    (m.Description != null && m.Description.ToLower().Contains(lowerSearch))
                );
            }

            filterRecord = query.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection)
                && !string.Equals(sortColumn, "No", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    query = query.OrderBy($"{sortColumn} {sortColumnDirection}");
                }
                catch
                {
                    query = query.OrderByDescending(m => m.DateOut);
                }
            }

            var pagedData = query.Skip(skip).Take(pageSize).ToList();

            var result = pagedData
              .Select((m, index) => new {
                  No = skip + index + 1,
                  Id = m.Id,
                  vehicle = new
                {
                    description = m.Vehicle.Description,
                    plate = m.Vehicle.Plate
                },
                dateOut = m.DateOut.ToString("dd/MM/yyyy"),
                kmOut = m.KmOut,
                dateIn = m.DateIn.ToString("dd/MM/yyyy"),
                kmIn = m.KmIn,
                driver = m.Driver,
                description = m.Description
            }).ToList();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = result
            });
        }

        [HttpGet]
        public IActionResult GetMaxKmIn(int vehicleId)
        {
            var maxKmIn = _context.Maintenances
                .Where(m => m.VehicleId == vehicleId)
                .Max(m => (int?)m.KmIn) ?? 0;

            return Json(new { maxKmIn });
        }
    }
}
