using System.Diagnostics;
using ConstructionApp.Data;
using ConstructionApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ConstructionApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace ConstructionApp.Controllers
{
    public class HomeController : AppController
    {
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _env;

        public HomeController(SignInManager<User> signInManager, UserManager<User> userManager,AppDbContext context, IWebHostEnvironment env)
            :base(userManager, context)
        {
            _signInManager = signInManager;
            _env = env;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            // User already logged in, redirect to home
            if (IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // First, try to find the user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (! model.Email.Contains("@"))
            {
                user = await _userManager.FindByNameAsync(model.Email);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            // Try to sign in
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "You are not allowed to login. Please confirm your email or contact support.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid password.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (! IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            var workSites = await _context.WorkSites
            .Select(ws => new WorkSiteCardViewModel
            {
                Id = ws.Id,
                Name = ws.Name,
                Picture = ws.Picture,
                Address = ws.Address,
                Amount = ws.Amount,
                Safety = ws.Safety,
                MaterialCost =
                    ws.Purchases
                        .Select(p => (float?)(p.Amount * p.Quantity))
                        .Sum() ?? 0,
                WorkerSalary =
                    _context.Presences
                        .Where(pr => pr.WorkSiteId == ws.Id)
                        .Select(pr => (float?)pr.Cost)
                        .Sum() ?? 0,
                DateStart = ws.DateStart,
                DateEnd = ws.DateEnd,
                ManagerName = ws.Manager.UserName ?? "",
            })
            .ToListAsync();

            if (IsWorker)
            {
                workSites = workSites.Where(ws => ws.Id == MyWorkSiteId).ToList();
            }

            var model = new DashboardViewModel
            {
                WorkSiteCards = workSites,
            };

            return View(model);
        }
         
        [Authorize]
        [HttpGet]
        public IActionResult ViewFile(string path)
        { 
            var fullPath = Path.Combine(_env.WebRootPath, path);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var contentType = GetContentType(fullPath);
            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            var fileName = Path.GetFileName(fullPath);

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
            return File(fileBytes, contentType); 
        }

        // Determine MIME type from file extension
        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".csv" => "text/csv",
                ".zip" => "application/zip",
                ".json" => "application/json",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                _ => "application/octet-stream", // fallback
            };
        }
         

        [HttpGet]
        public JsonResult GetDeadlineList(string? typeStr)
        {
            var maintenancedata = _context.Vehicles
                .AsEnumerable()
                .SelectMany(v => new[]
                {
                    new AttachFileViewModel{ Id = v.Id, Type = "Maintenance", Name = v.Plate, Renewed = false, Dumped = false,FileName="Picture",
                        StoredFilePath = v.Picture != null ? v.Picture : "\\images\\vehicle.svg", Deadline = v.DateInsurance, Description = "Insurance" },
                    new AttachFileViewModel{ Id = v.Id, Type = "Maintenance", Name = v.Plate, Renewed = false, Dumped = false,FileName="Picture",
                        StoredFilePath = v.Picture != null ? v.Picture : "\\images\\vehicle.svg", Deadline = v.DateRevision, Description = "Revision" },
                    new AttachFileViewModel{ Id = v.Id, Type = "Maintenance", Name = v.Plate, Renewed = false, Dumped = false,FileName="Picture",
                        StoredFilePath = v.Picture != null ? v.Picture : "\\images\\vehicle.svg", Deadline = v.DateMaintenance, Description = "Maintenance" }
                }).ToList();

            var attachdata = _context.AttachFiles
                .Where(x => ! x.Renewed)
                .Select(x => new AttachFileViewModel
                 {
                     Id = x.Id,
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
                 }).ToList();

            var data = attachdata.Concat(maintenancedata).AsQueryable()
                .Where(x => x.Deadline > DateTime.Now && x.Deadline < DateTime.Now.AddDays(7));
             
            if (typeStr != null)
            {
                data = data.Where(x => x.Type == typeStr);
            }

            var deadList = data
              .Select((v, index) => new {
                  no = index + 1,
                  id = v.Id,
                  name = v.Name,
                  description = v.Description,
                  deadline = v.Deadline.ToString("dd/MM/yyyy"),
                  storedFilePath = v.StoredFilePath
              }).ToList();

            var returnObj = new
            {
                data = deadList
            }; 

            return Json(returnObj);
        } 
    }
}