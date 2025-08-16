using ConstructionApp.Data;
using ConstructionApp.Models;
using ConstructionApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ConstructionApp.Helpers;

namespace ConstructionApp.Controllers
{
    public class PresencesController : AppController
    {
        public PresencesController(UserManager<User> userManager, AppDbContext context)
            : base(userManager, context)
        {

        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpGet]
        public async Task<IActionResult> Create(int workSiteId)
        {
            var model = new PresenceDailyViewModel
            {
                Date = DateTime.Today,
                Employees = await _context.Users
                    .Where(e => e.WorkSiteId == workSiteId && (e.RoleId == RoleIds.Manager || e.RoleId == RoleIds.Worker))
                    .Select(e => new PresenceEmployeeViewModel
                    {
                        EmployeeId = e.Id,
                        EmployeeName = e.UserName??"Unknown",
                        
                    }).ToListAsync()
            };

            return PartialView("_Create", model);
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PresenceDailyViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_Create", viewModel);
            }

            if (viewModel.Employees != null)
            {
                foreach (var emp in viewModel.Employees)
                {
                    if (emp.IsPresent)
                    {
                        var presence = new Presence
                        {
                            Date = viewModel.Date,
                            EmployeeId = emp.EmployeeId,
                            WorkSiteId = viewModel.WorkSiteId,
                            HS = emp.HS,
                            HR = emp.HR,
                            Cost = emp.Cost
                        };
                        _context.Presences.Add(presence);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        public JsonResult GetPresenceList()
        {
            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            // Base query
            var data = _context.Presences
                .Include(p => p.WorkSite)
                .Include(p => p.Employee)
                .AsQueryable();
            //get total count of data in table
            totalRecord = data.Count();

            if (IsTopLeader)
            {
                string? workSiteIdStr = Request.Form["workSiteDropDown"].FirstOrDefault();
                int? workSiteId = string.IsNullOrWhiteSpace(workSiteIdStr) ? null : int.Parse(workSiteIdStr);
                if (workSiteId == 0)
                {
                    data = data.Where(x => x.Employee.RoleId >= RoleIds.Manager);
                }
                else
                {
                    data = data.Where(x => x.WorkSiteId == workSiteId && x.Employee.RoleId >= RoleIds.Manager);
                }
            }
            else if (IsManager)
            {
                data = data.Where(x => x.WorkSiteId == MyWorkSiteId);
            }
            else
            {
                return Json(null);
            }

            // search data when search value found
            if (!string.IsNullOrEmpty(searchValue))
            {
                var lowerSearch = searchValue.ToLower();

                data = data.Where(x =>
                    x.WorkSite != null && x.WorkSite.Name.ToLower().Contains(lowerSearch) ||
                    x.Employee != null && x.Employee.UserName != null && x.Employee.UserName.ToLower().Contains(lowerSearch));
            }
            // get total count of records after search
            filterRecord = data.Count();
            //sort data
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection)
                && !string.Equals(sortColumn, "No", StringComparison.OrdinalIgnoreCase))
            {
                data = data.OrderBy($"{sortColumn} {sortColumnDirection}");
            }
            //pagination
            var pagedData = data.Skip(skip).Take(pageSize).ToList();

            var empList = pagedData
              .Select((p, index) => new {
                  no = skip + index + 1,
                  id = p.Id,
                  date = p.Date.ToString("dd/MM/yyyy"),
                  workSiteName = p.WorkSite.Name,
                  employeeName = p.Employee.UserName,
                  hs = p.HS,
                  hr = p.HR,
                  cost = CurrencyHelper.FormatEuro(p.Cost)
              }).ToList();

            var returnObj = new
            {
                draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = empList
            };

            return Json(returnObj);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var presence = await _context.Presences.FindAsync(id);
            if (presence == null)
                return NotFound();

            _context.Presences.Remove(presence);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _context.Presences
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (model == null)
                return NotFound();

            var viewModel = new PresenceEmployeeViewModel
            {
                Id = id,
                EmployeeId = model.EmployeeId,
                EmployeeName = model.Employee.UserName??"Unknown",
                HS = model.HS,
                HR = model.HR,
                Cost = model.Cost
            };

            return PartialView("_Edit", viewModel);
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PresenceEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_Edit", model);
            }

            var presence = await _context.Presences
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (presence == null)
            {
                return NotFound();
            }

            presence.HS = model.HS;
            presence.HR = model.HR;
            presence.Cost = model.Cost;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
