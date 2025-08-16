using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConstructionApp.Models
{
    public class Role : IdentityRole<int>
    {
        public Role() : base() { }
        public Role(string roleName) : base(roleName)
        {
        }
    }

    public static class RoleIds
    {
        public const int Admin = 1;
        public const int Surveyor = 2;
        public const int Manager = 3;
        public const int Worker = 4;
        public const int Supplier = 5;
        public const int Guest = 6;

        public static string ToString(int id)
        {
            return id switch
            {
                Admin => "Admin",
                Surveyor => "Surveyor",
                Manager => "Manager",
                Worker => "Worker",
                Supplier => "Supplier",
                _ => "Guest"
            };
        }

        public static int ToInt(string id)
        {
            return id switch
            {
                "Admin" => Admin,
                "Surveyor" => Surveyor,
                "Manager" => Manager,
                "Worker" => Worker,
                "Supplier" => Supplier,
                _ => 6
            };
        }

        public static SelectList All()
        {
            return new SelectList(new[]
            {
                new { Id = 1, Name = "Admin" },
                new { Id = 2, Name = "Surveyor" },
                new { Id = 3, Name = "Manager" },
                new { Id = 4, Name = "Worker" },
                new { Id = 5, Name = "Supplier" }
            }, "Id", "Name");
        }
    }
}