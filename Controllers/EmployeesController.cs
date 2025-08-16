using ConstructionApp.Data;
using ConstructionApp.Helpers;
using ConstructionApp.Models;
using ConstructionApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Dynamic.Core;

namespace ConstructionApp.Controllers
{
    public class EmployeesController : AppController
    {
        private readonly IWebHostEnvironment _env;

        public EmployeesController(UserManager<User> _userManager, AppDbContext _context, IWebHostEnvironment env)
            : base(_userManager, _context)
        {
            _env = env;
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        private async Task<string> SaveUploadedFileAsync(string plate, IFormFile formFile)
        {
            string ext = Path.GetExtension(formFile.FileName);
            string randomName = $"{Guid.NewGuid()}{ext}";
            string savePath = Path.Combine(_env.WebRootPath, "uploads\\employees", plate);
            Directory.CreateDirectory(savePath);

            string filePath = Path.Combine(savePath, randomName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await formFile.CopyToAsync(stream);
            return Path.Combine("uploads\\employees", plate, randomName);
        }

        [Authorize(Roles = "Admin,Surveyor")]
        [HttpGet]
        public IActionResult Create(int workSiteId)
        {
            var viewModel = new RegisterViewModel
            {
                WorkSites = new SelectList(_context.WorkSites
                    .Select(u => new { u.Id, u.Name })
                    .ToList(), "Id", "Name"),
            };

            return PartialView("_Create", viewModel);
        }

        [Authorize(Roles = "Admin,Surveyor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_Create", model);

            var user = new User
            {
                FullName = model.FullName,
                UserName = model.UserName,
                NormalizedUserName = model.UserName.ToUpper(),
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                WorkSiteId = model.WorkSiteId,
                RoleId = model.Id_Role??RoleIds.Worker,
                EncryptedPassword = EncryptionHelper.Encrypt(model.Password)
            };

            if (model.Avatar != null) { 
                user.Avatar = await SaveUploadedFileAsync(user.UserName, model.Avatar);
            }

            if(user.RoleId == RoleIds.Manager)
            {
                user.WorkSiteId = null;
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return PartialView("_Create", model);
            }
            var roleName = RoleIds.ToString(user.RoleId);
            await _userManager.AddToRoleAsync(user, roleName);

            if (model.AttachFiles != null)
            {
                foreach (var file in model.AttachFiles)
                {
                    if (file.Dumped == false && file.FormFile != null && file.FormFile.Length > 0)
                    {
                        string storedName = await SaveUploadedFileAsync(model.UserName, file.FormFile);

                        var savedFile = new AttachFile
                        {
                            FileName = file.FileName,
                            StoredFilePath = storedName,
                            Description = file.Description,
                            Deadline = file.Deadline,
                            Renewed = file.Renewed,
                            UserId = user.Id
                        };

                        _context.AttachFiles.Add(savedFile);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Employees");
        }


        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        public JsonResult GetEmployeeList()
        {
            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            var data = _context.Users
                .Include(p => p.WorkSite)
                .AsQueryable();
                 
            if (IsTopLeader)
            {
                string? workSiteIdStr = Request.Form["workSiteDropDown"].FirstOrDefault();
                int? workSiteId = string.IsNullOrWhiteSpace(workSiteIdStr) ? null : int.Parse(workSiteIdStr);
                if (workSiteId == 0)
                {
                    data = data.Where(x => x.RoleId > MyRoleId);
                }
                else { 
                    data = data.Where(x => x.WorkSiteId == workSiteId && x.RoleId > MyRoleId);
                }
            }
            else if(IsManager)
            {
                data = data.Where(
                    x => x.WorkSiteId == MyWorkSiteId &&
                    x.RoleId > RoleIds.Manager);
            }
            else
            {
                return Json(null);
            }

            //get total count of data in table
            totalRecord = data.Count();
            // search data when search value found
            if (!string.IsNullOrEmpty(searchValue))
            {
                var lowerSearch = searchValue.ToLower();  
                data = data.Where(x =>
                    x.UserName != null && x.UserName.ToLower().Contains(lowerSearch) ||
                    x.FullName != null && x.FullName.ToLower().Contains(lowerSearch) ||
                    x.Address != null && x.Address.ToLower().Contains(lowerSearch) ||
                    x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(lowerSearch) 
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
              .Select((x, index) => new {
                  no = skip + index + 1,
                  x.Id,
                  x.UserName,
                  x.FullName,
                  x.Address,
                  roleId = (RoleIds.ToString(x.RoleId)),
                  x.Email,
                  workSite = x.WorkSite != null ? x.WorkSite.Name : ""
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

        [Authorize(Roles = "Admin,Surveyor")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users
               .Include(u => u.AttachFiles)
               .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var model = new RegisterViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Address = user.Address ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Password = EncryptionHelper.Decrypt(user.EncryptedPassword),
                WorkSiteId = user.WorkSiteId,
                Id_Role = user.RoleId,
                AvatarUrl = user.Avatar,
                WorkSites = new SelectList(await _context.WorkSites.ToListAsync(), "Id", "Name"),
                AttachFiles = user.AttachFiles.Select(f => new AttachFileViewModel
                {
                    TempId = f.Id,
                    FileName = f.FileName,
                    Description = f.Description,
                    Deadline = f.Deadline,
                    Renewed = f.Renewed,
                    Dumped = false,
                    StoredFilePath = f.StoredFilePath
                }).ToList()
            };

            return PartialView("_Edit", model);
        }

        [Authorize(Roles = "Admin,Surveyor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_Edit", model);
            }

            var user = await _context.Users
                .Include(u => u.AttachFiles)
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            if (user == null)
                return NotFound();

            // Update user properties
            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.NormalizedUserName = model.UserName.ToUpper();
            user.Email = model.Email;
            user.NormalizedEmail = model.Email.ToUpper();
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.WorkSiteId = model.WorkSiteId;
            user.RoleId = model.Id_Role?? RoleIds.Worker;
            user.EncryptedPassword = EncryptionHelper.Encrypt(model.Password);

            if (model.Avatar != null)
            {
                if (user.Avatar != null)
                {
                    var path = _env.WebRootPath + user.Avatar;
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path); 
                }
                user.Avatar = await SaveUploadedFileAsync(user.UserName, model.Avatar);
            }

            // Remove files that are no longer in the model
            var filesToRemove = user.AttachFiles
                .Where(f => model.AttachFiles.Any(mf => mf.TempId == f.Id && mf.Dumped))
                .ToList();

            foreach (var file in filesToRemove)
            {
                var path = _env.WebRootPath + file.StoredFilePath;
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                _context.AttachFiles.Remove(file);
            }

            // Update existing files and add new ones
            if (model.AttachFiles != null)
            {
                foreach (var file in model.AttachFiles)
                {
                    if (file.TempId != null)
                    {
                        var existing = user.AttachFiles.FirstOrDefault(f => f.Id == file.TempId);
                        if (existing != null)
                        {
                            // Update metadata for existing file
                            existing.Description = file.Description;
                            existing.FileName = file.FileName;
                            existing.Deadline = file.Deadline;
                            existing.Renewed = file.Renewed;
                        }
                    }
                    else if (file.FormFile != null && file.FormFile.Length > 0)
                    {
                        string storedName = await SaveUploadedFileAsync(model.UserName, file.FormFile);

                        var savedFile = new AttachFile
                        {
                            FileName = file.FileName,
                            StoredFilePath = storedName,
                            Description = file.Description,
                            Deadline = file.Deadline,
                            Renewed = file.Renewed,
                            UserId = model.Id
                        };
                        user.AttachFiles.Add(savedFile);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Employees");
        }


        // POST: Employee/Delete/5
        [Authorize(Roles = "Admin, Surveyor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {

            var user = await _context.Users.Include(u => u.AttachFiles).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            if (user.RoleId == RoleIds.Admin)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            if(user.WorkSiteId != null)
            {
                return BadRequest(new { error = "Cannot delete user because they are assigned as a manager to one or more work sites." });
            }

            var presences = _context.Presences.Where(p => p.EmployeeId == id);
            _context.Presences.RemoveRange(presences);

            foreach (var file in user.AttachFiles)
            {
                var path = _env.WebRootPath + file.StoredFilePath;
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            _context.AttachFiles.RemoveRange(user.AttachFiles);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Employees");
        }

        [Authorize(Roles = "Admin,Surveyor")]
        [HttpGet]
        public JsonResult SearchFreeWorkers(string term)
        {
            var workers = _context.Users
                .Where(u => u.RoleId == RoleIds.Worker && u.WorkSiteId == null && (term.IsNullOrEmpty() || (u.UserName != null && u.UserName.Contains(term))))
                .Select(u => new
                {
                    label = u.UserName,
                    value = u.UserName,
                    id = u.Id
                })
                .Take(10)
                .ToList();

            return Json(workers);
        }

        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            var user = await _context.Users
               .Include(u => u.AttachFiles)
               .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }
            else if (MyRoleId >= user.RoleId)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var model = new RegisterViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Address = user.Address ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Password = EncryptionHelper.Decrypt(user.EncryptedPassword),
                WorkSiteId = user.WorkSiteId,
                Id_Role = user.RoleId,
                AvatarUrl = user.Avatar,
                WorkSites = new SelectList(await _context.WorkSites.ToListAsync(), "Id", "Name"),
                AttachFiles = user.AttachFiles.Select(f => new AttachFileViewModel
                {
                    TempId = f.Id,
                    FileName = f.FileName,
                    Description = f.Description,
                    Deadline = f.Deadline,
                    Renewed = f.Renewed,
                    Dumped = false,
                    StoredFilePath = f.StoredFilePath
                }).ToList()
            };

            return PartialView("_Profile", model);
        }
    }
}
