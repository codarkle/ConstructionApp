using ConstructionApp.ViewModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConstructionApp.Models;
using ConstructionApp.Data;
using Microsoft.AspNetCore.Identity;

namespace ConstructionApp.Controllers
{
    public abstract class AppController : Controller
    {
        protected readonly UserManager<User> _userManager;
        protected readonly AppDbContext _context;
        public AppController(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
        protected bool IsTopLeader => User.IsInRole("Admin") || User.IsInRole("Surveyor");
        protected bool IsSupplier => User.IsInRole("Supplier");
        protected bool IsManager => User.IsInRole("Manager") && MyWorkSiteId != null;
        protected bool IsWorker => User.IsInRole("Worker") && MyWorkSiteId != null;
        protected int? MyWorkSiteId
        {
            get
            {
                var userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : (int?)null;
                if (userId == null) return null;

                return _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.WorkSiteId)
                    .FirstOrDefault();
            }
        }

        protected int? MyRoleId
        {
            get
            {
                var userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : (int?)null;
                if (userId == null) return null;

                return _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.RoleId)
                    .FirstOrDefault();
            }
        }
         
        public override void OnActionExecuting(ActionExecutingContext context)
        { 

            var userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : (int?)null;
            if (userId != null)
            { 
                ViewBag.MyWorkSiteId = MyWorkSiteId;
            }
            if (User.IsInRole("Admin"))
            {
                ViewBag.MyRole = "Admin";
            }
            else if (User.IsInRole("Surveyor"))
            {
                ViewBag.MyRole = "Surveyor";
            }
            else if (IsManager)
            {
                ViewBag.MyRole = "Manager";
            }
            else if(IsAuthenticated)
            {
                if (MyWorkSiteId != null)
                {
                    ViewBag.MyRole = "Employee";
                }
                else
                {
                    ViewBag.MyRole = "InActive";
                }
            }
            else {
                ViewBag.MyRole = "Unknown";
            }
            base.OnActionExecuting(context);
        }
    }
}
