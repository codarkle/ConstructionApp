using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConstructionApp.Data;
using ConstructionApp.Models;
using ConstructionApp.ViewModels;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Azure;

namespace ConstructionApp.Controllers
{
    public class VehiclesController : AppController
    {
        private readonly IWebHostEnvironment _env;
        public VehiclesController(UserManager<User> userManager, AppDbContext context, IWebHostEnvironment env)
            : base(userManager, context)
        {
            _env = env;
        }

        [Authorize(Roles = "Admin,Surveyor")]
        public IActionResult Index()
        {
            return View();
        }

        private async Task<string> SaveUploadedFileAsync(string plate, IFormFile formFile)
        {
            var ext = Path.GetExtension(formFile.FileName);
            var randomName = $"{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(_env.WebRootPath, "uploads\\vehicles", plate);
            Directory.CreateDirectory(savePath);

            var filePath = Path.Combine(savePath, randomName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await formFile.CopyToAsync(stream);

            return Path.Combine("uploads\\vehicles", plate, randomName);
        }


        [Authorize(Roles = "Admin,Surveyor")]
        public IActionResult Create()
        {
            return PartialView("_Create", new VehicleViewModel
            {
                DateInsurance = DateTime.Now.AddMonths(1),
                DateRevision = DateTime.Now.AddMonths(2),
                DateMaintenance = DateTime.Now.AddMonths(3)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Surveyor")]
        public async Task<IActionResult> Create(VehicleViewModel model)
        { 
            if (!ModelState.IsValid)
            {
                return PartialView("_Create", model);
            }

            var vehicle = new Vehicle
            {
                Description = model.Description,
                Plate = model.Plate,
                DateInsurance = model.DateInsurance,
                DateRevision = model.DateRevision,
                DateMaintenance = model.DateMaintenance,
            };

            if (model.Picture != null)
            {
                vehicle.Picture = await SaveUploadedFileAsync(vehicle.Plate, model.Picture);
            }

            if (model.AttachFiles != null)
            {
                foreach (var file in model.AttachFiles)
                {
                    if (file.Dumped == false && file.FormFile != null && file.FormFile.Length > 0)
                    {
                        var savedFile = await SaveUploadedFileAsync(model.Plate, file.FormFile);
                        vehicle.AttachFiles.Add(new AttachFile
                        {
                            FileName = file.FileName,
                            StoredFilePath = savedFile,
                            Description = file.Description,
                            Deadline = file.Deadline
                        });
                    }
                }
            }

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Vehicles");
        }

        [Authorize(Roles = "Admin,Surveyor")]
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.AttachFiles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null) return NotFound();

            return PartialView("_Edit", new VehicleViewModel
            {
                Id = vehicle.Id,
                Description = vehicle.Description,
                Plate = vehicle.Plate,
                PicturePath = vehicle.Picture,
                DateInsurance = vehicle.DateInsurance,
                DateRevision = vehicle.DateRevision,
                DateMaintenance = vehicle.DateMaintenance,
                AttachFiles = vehicle.AttachFiles.Select(f => new AttachFileViewModel
                {
                    TempId = f.Id,
                    FileName = f.FileName,
                    Description = f.Description,
                    Deadline = f.Deadline,
                    StoredFilePath = f.StoredFilePath
                }).ToList()
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Surveyor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VehicleViewModel model)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.AttachFiles)
                .FirstOrDefaultAsync(v => v.Id == model.Id);

            if (vehicle == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.PicturePath = vehicle.Picture;
                return PartialView("_Edit", model);
            }

            vehicle.Description = model.Description;
            vehicle.Plate = model.Plate;
            vehicle.DateInsurance = model.DateInsurance;
            vehicle.DateRevision = model.DateRevision;
            vehicle.DateMaintenance = model.DateMaintenance;

            if (model.Picture != null)
            {
                if (vehicle.Picture != null)
                {
                    var path = _env.WebRootPath + vehicle.Picture;
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                vehicle.Picture = await SaveUploadedFileAsync(vehicle.Plate, model.Picture);
            }

            var filesToRemove = vehicle.AttachFiles
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
                            var existing = vehicle.AttachFiles.FirstOrDefault(f => f.Id == file.TempId);
                            if (existing != null)
                            {
                                existing.Description = file.Description;
                                existing.FileName = file.FileName;
                                existing.Deadline = file.Deadline;
                            }
                        }
                        else if (file.FormFile != null && file.FormFile.Length > 0)
                        {
                            var savedFile = await SaveUploadedFileAsync(model.Plate, file.FormFile);
                            vehicle.AttachFiles.Add(new AttachFile
                            {
                                FileName = file.FileName,
                                StoredFilePath = savedFile,
                                Description = file.Description,
                                Deadline = file.Deadline
                            });
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Vehicles");
        }


        [Authorize(Roles = "Admin,Surveyor")]
        public async Task<IActionResult> View(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.AttachFiles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null) return NotFound();

            return PartialView("_View", new VehicleViewModel
            {
                Id = vehicle.Id,
                Description = vehicle.Description,
                Plate = vehicle.Plate,
                PicturePath = vehicle.Picture,
                DateInsurance = vehicle.DateInsurance,
                DateRevision = vehicle.DateRevision,
                DateMaintenance = vehicle.DateMaintenance,
                AttachFiles = vehicle.AttachFiles.Select(f => new AttachFileViewModel
                {
                    TempId = f.Id,
                    FileName = f.FileName,
                    Description = f.Description,
                    Deadline = f.Deadline,
                    StoredFilePath = f.StoredFilePath
                }).ToList()
            });
        }

        // POST: Vehicles/DeleteConfirmed
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.AttachFiles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null) return NotFound();

            // Delete attached files from disk
            foreach (var file in vehicle.AttachFiles)
            {
                var path = _env.WebRootPath + file.StoredFilePath;
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path); 
            }

            // Explicitly remove VehicleAttachFiles if not using cascade
            _context.AttachFiles.RemoveRange(vehicle.AttachFiles);

            // Remove the vehicle
            _context.Vehicles.Remove(vehicle);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Vehicles");
        }

        [Authorize(Roles = "Admin,Surveyor")]
        [HttpPost]
        public JsonResult GetVehicleList()
        {
            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var data = _context.Vehicles.AsQueryable();
            //get total count of data in table
            totalRecord = data.Count();
            // search data when search value found
            if (!string.IsNullOrEmpty(searchValue))
            {
                var lowerSearch = searchValue.ToLower();

                data = data.Where(x =>
                    x.Description != null && x.Description.ToLower().Contains(lowerSearch) ||
                    x.Plate != null && x.Plate.ToLower().Contains(lowerSearch)
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
              .Select((v, index) => new {
                  no = skip + index + 1,
                  id = v.Id,
                  description = v.Description,
                  plate = v.Plate,
                  dateInsurance = v.DateInsurance.ToString("dd/MM/yyy"),
                  dateRevision = v.DateRevision.ToString("dd/MM/yyy"),
                  dateMaintenance = v.DateMaintenance.ToString("dd/MM/yyy")
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
    }
}
