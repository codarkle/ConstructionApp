using ConstructionApp.Data;
using ConstructionApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConstructionApp.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            // Define your roles
            string[] roles = { "Admin", "Surveyor", "Manager", "Worker", "Supplier" };

            foreach (var role in roles)
            {
                await AddRoleAsync(roleManager, role);
            }

            try{
                // Ensure the database is ready
                logger.LogInformation("Ensuring the database is created.");
                await context.Database.EnsureCreatedAsync();

                //await CreateUserWithRoleAsync(userManager, "Admin@example.com", "Admin", "SuperAdmin", 1, "password");
                await SeedUsersFromCsvAsync(userManager, Path.Combine("wwwroot/uploads", "users.csv"));
                await SeedMaterialsAsync(context, logger);
                await SeedVehiclesAsync(context, logger);
                await SeedMaintenanceVehiclesAsync(context, logger);

                logger.LogInformation("User seeding completed.");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        public static async Task SeedUsersFromCsvAsync(UserManager<User> userManager, string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException($"CSV file not found at path: {csvFilePath}");

            var lines = await File.ReadAllLinesAsync(csvFilePath);

            foreach (var line in lines.Skip(1)) // Skip header
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var columns = line.Split(',');

                if (columns.Length < 5)
                    continue;

                string email = columns[0].Trim();
                string username = columns[1].Trim();
                string fullName = columns[2].Trim();
                if (!int.TryParse(columns[3].Trim(), out int roleId))
                    continue;

                string password = columns[4].Trim();

                await CreateUserWithRoleAsync(userManager, email, username, fullName, roleId, password);
            }
        }

        public static async Task SeedMaterialsAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Materials.AnyAsync())
            {
                logger.LogInformation("Materials already exist, skipping seeding.");
                return;
            }

            var materials = new List<Material>
            {
                new Material { Name = "Cement", Description = "The most important material"},
                new Material { Name = "Steel",Description = "It makes buildings strong" },
                new Material { Name = "Sand", Description = "Sand is necessary" },
                new Material { Name = "Bricks", Description = "Factories make it." }
            };

            context.Materials.AddRange(materials);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded materials.");
        }

        public static async Task SeedVehiclesAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Vehicles.AnyAsync())
            {
                logger.LogInformation("Vehicles already exist, skipping seeding.");
                return;
            }

            var vehicles = new List<Vehicle>
             {
                new Vehicle { Description = "Pickup Truck", Plate = "ABC-1234", DateInsurance = DateTime.Now.AddMonths(-6), DateRevision = DateTime.Now.AddMonths(6), DateMaintenance = DateTime.Now.AddMonths(3) },
                new Vehicle { Description = "Dump Truck", Plate = "XYZ-5678", DateInsurance = DateTime.Now.AddMonths(-4), DateRevision = DateTime.Now.AddMonths(8), DateMaintenance = DateTime.Now.AddMonths(5) },
                new Vehicle { Description = "Crane", Plate = "CRN-9012", DateInsurance = DateTime.Now.AddMonths(-2), DateRevision = DateTime.Now.AddMonths(10), DateMaintenance = DateTime.Now.AddMonths(7) },
                new Vehicle { Description = "Tractor", Plate = "TRA-2923", DateInsurance = DateTime.Now.AddMonths(-3), DateRevision = DateTime.Now.AddMonths(8), DateMaintenance = DateTime.Now.AddMonths(10) }
            };

            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded vehicles.");
        }

        public static async Task SeedMaintenanceVehiclesAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Maintenances.AnyAsync())
            {
                logger.LogInformation("Maintenance records already exist, skipping seeding.");
                return;
            }

            // Make sure you have vehicles in the database first
            var vehicles = await context.Vehicles.Take(3).ToListAsync();

            if (vehicles.Count == 0)
            {
                logger.LogWarning("No vehicles found. Please seed vehicles before maintenance records.");
                return;
            }

            var maintenanceRecords = new List<Maintenance>
            {
                new Maintenance
                {
                    VehicleId = vehicles[0].Id,
                    DateOut = DateTime.Now.AddDays(-30),
                    KmOut = 10000,
                    DateIn = DateTime.Now.AddDays(-25),
                    KmIn = 10100,
                    Driver = "John Doe",
                    Description = "Oil change and tire rotation"
                },
                new Maintenance
                {
                    VehicleId = vehicles[1].Id,
                    DateOut = DateTime.Now.AddDays(-20),
                    KmOut = 20000,
                    DateIn = DateTime.Now.AddDays(-15),
                    KmIn = 20100,
                    Driver = "Jane Smith",
                    Description = "Brake pads replacement"
                },
                new Maintenance
                {
                    VehicleId = vehicles[1].Id,
                    DateOut = DateTime.Now.AddDays(-10),
                    KmOut = 12000,
                    DateIn = DateTime.Now.AddDays(-3),
                    KmIn = 15000,
                    Driver = "John Carter",
                    Description = "Change wheels"
                },
                new Maintenance
                {
                    VehicleId = vehicles[2].Id,
                    DateOut = DateTime.Now.AddDays(-10),
                    KmOut = 30000,
                    DateIn = DateTime.Now.AddDays(-5), // Still out for maintenance
                    KmIn = 32000,
                    Driver = "Jim Brown",
                    Description = "Engine diagnostics"
                }
            };

            context.Maintenances.AddRange(maintenanceRecords);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded maintenance vehicle records.");
        }

        private static async Task AddRoleAsync(RoleManager<Role> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new Role(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private static async Task CreateUserWithRoleAsync(UserManager<User> userManager, string email, string username, string fullname, int roleId, string password)
        {
            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var user = new User
            {
                UserName = username,
                Email = email,
                FullName = fullname,
                RoleId = roleId,
                Address = "x",
                PhoneNumber = "0",
                WorkSiteId = null,
                EncryptedPassword = EncryptionHelper.Encrypt(password)
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var roleName = RoleIds.ToString(roleId);
            await userManager.AddToRoleAsync(user, roleName);
        }
    }
}
