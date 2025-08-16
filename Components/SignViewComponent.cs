using Microsoft.AspNetCore.Mvc;
using ConstructionApp.Data;
using ConstructionApp.ViewModels;
using ConstructionApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Linq;

namespace ConstructionApp.Components
{
    public class SignViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly SignInManager<User> _signInManager;

        public SignViewComponent(AppDbContext context, SignInManager<User> manager)
        {
            _context = context;
            _signInManager = manager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new SignViewModel();

            var currentUser = HttpContext.User;
            if (_signInManager.IsSignedIn(currentUser))
            {
                var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        model.userName = user.FullName;
                        model.userRole = RoleIds.ToString(user.RoleId);
                        model.userAvatar = !string.IsNullOrEmpty(user.Avatar)
                            ? "~/" + user.Avatar
                            : $"~/images/avatars/{model.userRole}.svg";
                    }
                }
            }
            return View(model);
        }
    }
}
