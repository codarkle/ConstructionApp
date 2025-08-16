using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConstructionApp.Data;
using ConstructionApp.Models;
using ConstructionApp.ViewModels;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using ConstructionApp.Helpers;

namespace ConstructionApp.Controllers
{
    public class WorkSitesController : AppController
    {
        private readonly IWebHostEnvironment _env;
        public WorkSitesController(UserManager<User> userManager, AppDbContext context, IWebHostEnvironment env)
            : base(userManager, context) 
        {
            _env = env;
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        public IActionResult Index()
        {
            if (IsTopLeader)
            {
                return View();
            }
            else if (IsManager)
            {
                return RedirectToAction("SiteManage", "WorkSites");
            } 
            else
            {
                return RedirectToAction("AccessDenied", "Home");
            }
        }

        private async Task<string> SaveUploadedFileAsync(string siteName, IFormFile formFile)
        {
            var ext = Path.GetExtension(formFile.FileName);
            var randomName = $"{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(_env.WebRootPath, "uploads\\worksites", siteName);
            Directory.CreateDirectory(savePath);

            var filePath = Path.Combine(savePath, randomName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await formFile.CopyToAsync(stream);

            return Path.Combine("uploads\\worksites", siteName, randomName);
        }

        [Authorize(Roles = "Admin,Surveyor")] 
        public async Task<IActionResult> Create()
        {
            var model = new WorkSiteViewModel
            {
                ManagerList = new SelectList(
                    await _context.Users.Where(u => u.RoleId == RoleIds.Manager && u.WorkSiteId == null).ToListAsync(),
                    "Id", "UserName"),
                DateStart = DateTime.Now,
                DateEnd = DateTime.Now.AddMonths(1)
            };

            return PartialView("_Create",model);
        }

        [Authorize(Roles = "Admin,Surveyor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkSiteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ManagerList = new SelectList(
                    await _context.Users.Where(u => u.RoleId == RoleIds.Manager && u.WorkSiteId == null).ToListAsync(),
                    "Id", "UserName");
                return PartialView("_Create", model);
            }

            var workSite = new WorkSite
            {
                Name = model.Name,
                Address = model.Address,
                CAP = model.CAP,
                Amount = model.Amount,
                Safety = model.Safety,
                DateStart = model.DateStart,
                DateEnd = model.DateEnd,
                ManagerId = model.ManagerId,
            };

            if (model.Picture != null)
            {
                workSite.Picture = await SaveUploadedFileAsync(workSite.Name, model.Picture);
            }

            // Add assigned workers
            if (model.SelectedWorkerIds?.Any() == true)
            {
                workSite.Workers = _context.Users
                    .Where(u => model.SelectedWorkerIds.Contains(u.Id))
                    .ToList();
            }
            if (model.AttachFiles != null)
            {
                foreach (var file in model.AttachFiles)
                {
                    if (file.Dumped == false && file.FormFile != null && file.FormFile.Length > 0)
                    {
                        var savedFile = await SaveUploadedFileAsync(model.Name, file.FormFile);
                        workSite.AttachFiles.Add(new AttachFile
                        {
                            FileName = file.FormFile.FileName,
                            StoredFilePath = savedFile,
                            Description = file.Description,
                            Author = file.Author,
                            Deadline = file.Deadline,
                        });
                    }
                }
            }

            _context.WorkSites.Add(workSite);
            await _context.SaveChangesAsync(); // so workSite.Id is generated
            // Manually assign the WorkSiteId to the selected manager
            var manager = await _context.Users.FindAsync(model.ManagerId);
            if (manager != null) {
                manager.WorkSiteId = workSite.Id;
                _context.Users.Update(manager);
                await _context.SaveChangesAsync(); // so workSite.Id is generated
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Surveyor")]
        public async Task<IActionResult> Edit(int id)
        {
            var worksite = await _context.WorkSites
                .Include(v => v.AttachFiles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (worksite == null) return NotFound();

            return PartialView("_Edit", new WorkSiteViewModel
            {
                Id = worksite.Id,
                Name = worksite.Name,
                Address = worksite.Address,
                PicturePath = worksite.Picture,
                CAP = worksite.CAP,
                Amount = worksite.Amount,
                Safety = worksite.Safety,
                DateStart = worksite.DateStart,
                DateEnd = worksite.DateEnd,
                ManagerId = worksite.ManagerId,
                AssignedWorkers = await _context.Users.Where(u => u.RoleId == RoleIds.Worker && (u.WorkSiteId == id)).ToListAsync(),
                SelectedWorkerIds = await _context.Users.Where(u => u.RoleId == RoleIds.Worker && (u.WorkSiteId == id)).Select(u=>u.Id).ToListAsync(),
                ManagerList = new SelectList(
                    await _context.Users.Where(u => u.RoleId == RoleIds.Manager && (u.WorkSiteId == null || u.WorkSiteId == id)).ToListAsync(),
                    "Id", "UserName"),
                AttachFiles = worksite.AttachFiles.Select(f => new AttachFileViewModel
                {
                    TempId = f.Id,
                    FileName = f.FileName,
                    Description = f.Description,
                    Author = f.Author,
                    Deadline = f.Deadline,
                    StoredFilePath = f.StoredFilePath
                }).ToList()
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Surveyor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WorkSiteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var site = await _context.WorkSites
                    .Include(v => v.AttachFiles)
                    .FirstOrDefaultAsync(v => v.Id == model.Id);

                if (site == null) return NotFound();

                model.ManagerList = new SelectList(
                    await _context.Users.Where(u => u.RoleId == RoleIds.Manager && (u.WorkSiteId == null || u.WorkSiteId == model.Id)).ToListAsync(),
                    "Id", "UserName");
                model.PicturePath = site.Picture;
                model.AssignedWorkers = await _context.Users.Where(u => u.RoleId == RoleIds.Worker && (u.WorkSiteId == site.Id)).ToListAsync();
                model.SelectedWorkerIds = await _context.Users.Where(u => u.RoleId == RoleIds.Worker && (u.WorkSiteId == site.Id)).Select(u => u.Id).ToListAsync();
                return PartialView("_Edit", model);
            }

            var worksite = await _context.WorkSites
                .Include(v => v.AttachFiles)
                .FirstOrDefaultAsync(v => v.Id == model.Id);

            if (worksite == null) return NotFound();

            worksite.Name = model.Name;
            worksite.Address = model.Address;
            worksite.CAP = model.CAP;
            worksite.Amount = model.Amount;
            worksite.Safety = model.Safety;
            worksite.DateStart = model.DateStart;
            worksite.DateEnd = model.DateEnd;

            if (model.Picture != null)
            {
                if (worksite.Picture != null)
                {
                    var path = _env.WebRootPath + worksite.Picture;
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                worksite.Picture = await SaveUploadedFileAsync(worksite.Name, model.Picture);
            }
             
            var filesToRemove = worksite.AttachFiles
                .Where(f => model.AttachFiles.Any(mf => mf.TempId == f.Id && mf.Dumped))
                .ToList();

            foreach (var f in filesToRemove)
            {
                var path = _env.WebRootPath + f.StoredFilePath;
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path); 

                _context.AttachFiles.Remove(f);
            }

            if (model.AttachFiles != null)
            {
                foreach (var file in model.AttachFiles)
                {
                    if (!file.Dumped)
                    {
                        if (file.TempId != null)
                        {
                            var existing = worksite.AttachFiles.FirstOrDefault(f => f.Id == file.TempId);
                            if (existing != null)
                            {
                                existing.Description = file.Description;
                                existing.FileName = file.FileName;
                                existing.Deadline = file.Deadline;
                                existing.Author = file.Author;
                            }
                        }
                        else if (file.FormFile != null && file.FormFile.Length > 0)
                        {
                            var savedFile = await SaveUploadedFileAsync(model.Name, file.FormFile);
                            worksite.AttachFiles.Add(new AttachFile
                            {
                                FileName = file.FileName,
                                StoredFilePath = savedFile,
                                Description = file.Description,
                                Deadline = file.Deadline,
                                Author = file.Author
                            });
                        }
                    }
                }
            }

            var usersToRemove = _context.Users
                .Where(u => u.WorkSiteId == worksite.Id && u.RoleId == RoleIds.Worker && !model.SelectedWorkerIds.Contains(u.Id))
                .ToList();

            foreach (var user in usersToRemove)
            {
                user.WorkSiteId = null;
            }

            if (model.SelectedWorkerIds != null)
            {
                worksite.Workers = _context.Users
                    .Where(u => model.SelectedWorkerIds.Contains(u.Id) && u.RoleId == RoleIds.Worker)
                    .ToList();
            }

            var oldManager = await _context.Users.FindAsync(worksite.ManagerId);
            if (oldManager != null)
            {
                oldManager.WorkSiteId = null;
            }
            var newManager = await _context.Users.FindAsync(model.ManagerId);
            if (newManager != null)
            {
                newManager.WorkSiteId = worksite.Id;
            }
            worksite.ManagerId = model.ManagerId;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "WorkSites");
        }

        [HttpGet]
        public JsonResult GetMySites()
        {
            IQueryable<WorkSite> allsites;

            if (IsTopLeader)
            {
                allsites = _context.WorkSites
                   .Include(w => w.Manager);
            }
            else if (MyWorkSiteId != null)
            {
                allsites = _context.WorkSites
                    .Include(w => w.Manager)
                    .Where(u => u.Id == MyWorkSiteId);
            }
            else
            {
                return Json(null); // Better to return Json(null) than just null
            }

            var mysites = allsites.Select(w => new
            {
                id = w.Id,
                name = w.Name,
                address = w.Address,
                managerName = w.Manager.UserName,
                amount = w.Amount,
                dateStart = w.DateStart.ToString("dd/MM/yyy"),
                dateEnd = w.DateEnd.ToString("dd/MM/yyy")
            }).ToList();

            return Json(mysites);
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        public async Task<IActionResult> SiteManage(int? id)
        {
            if (IsManager){
                if (id == null)
                {
                    id = MyWorkSiteId;
                }
                else if(MyWorkSiteId != id)
                {
                   return RedirectToAction("AccessDenied", "Home");
                }
            }
            if (id != null){
                var workSite = await _context.WorkSites
                    .FirstOrDefaultAsync(w => w.Id == id);
                if (workSite != null)
                {
                    ViewBag.NetProfit = await _context.WorkSites
                        .Where(ws => ws.Id == id)
                        .Select(ws =>
                            (float)(ws.Amount + ws.Safety) -
                            (float)(
                                ws.Purchases.Sum(p => p.Amount * p.Quantity) +
                                _context.Presences.Where(pr => pr.WorkSiteId == ws.Id).Sum(pr => pr.Cost)
                            )
                        )
                        .FirstOrDefaultAsync();

                    return View(workSite);
                }
            }
             
            return RedirectToAction("AccessDenied", "Home");
        }

        [Authorize(Roles = "Admin,Surveyor")]
        [HttpPost]
        public JsonResult GetWorkSiteList()
        {
            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var data = _context.WorkSites  
                .Include(ws => ws.Manager)
                .AsQueryable();
            //get total count of data in table
            totalRecord = data.Count();
            // search data when search value found
            if (!string.IsNullOrEmpty(searchValue))
            {
                var lowerSearch = searchValue.ToLower();

                data = data.Where(x =>
                    x.Name != null && x.Name.ToLower().Contains(lowerSearch) ||
                    x.Address != null && x.Address.ToLower().Contains(lowerSearch) ||
                    x.CAP != null && x.CAP.ToLower().Contains(lowerSearch) ||
                    x.Manager.UserName != null && x.Manager.UserName.ToLower().Contains(lowerSearch)
                );
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
              .Select((w, index) => new {
                  no = skip + index + 1,
                  id = w.Id,
                  name = w.Name,
                  address = w.Address,
                  cAP = w.CAP,
                  managerName = w.Manager.UserName,
                  amount = CurrencyHelper.FormatEuro(w.Amount),
                  safety = CurrencyHelper.FormatEuro(w.Safety),
                  dateStart = w.DateStart.ToString("dd/MM/yyyy"),
                  dateEnd = w.DateEnd.ToString("dd/MM/yyyy")
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

        // POST: WorkSites/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var workSite = await _context.WorkSites
                .Include(w => w.AttachFiles)
                .Include(w => w.Purchases)
                .Include(w => w.Presences)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workSite == null)
            {
                return NotFound();
            }

            var usersToUpdate = _context.Users.Where(u => u.WorkSiteId == id).ToList();
            foreach (var user in usersToUpdate)
            {
                user.WorkSiteId = null;
            }

            var presences = await _context.Presences
                .Where(p => p.WorkSiteId == id)
                .ToListAsync();
            _context.Presences.RemoveRange(presences);

            // Optional: remove dependent records manually if cascade is not configured
            _context.AttachFiles.RemoveRange(workSite.AttachFiles);
            _context.Purchases.RemoveRange(workSite.Purchases);

            _context.WorkSites.Remove(workSite);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}