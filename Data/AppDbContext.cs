using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ConstructionApp.Models;

namespace ConstructionApp.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<AttachFile> AttachFiles {  get; set; }
        public DbSet<WorkSite> WorkSites { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Presence> Presences { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<AttachFile>()
                .HasOne(f => f.User)
                .WithMany(u => u.AttachFiles)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttachFile>()
                .HasOne(f => f.Vehicle)
                .WithMany(v => v.AttachFiles)
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AttachFile>()
                .HasOne(f => f.WorkSite)
                .WithMany(w => w.AttachFiles)
                .HasForeignKey(f => f.WorkSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Maintenance>()
                .HasOne(mv => mv.Vehicle)
                .WithMany()
                .HasForeignKey(mv => mv.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WorkSite>()
                .HasOne(ws => ws.Manager)
                .WithMany()
                .HasForeignKey(ws => ws.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<User>()
                .HasOne(u => u.WorkSite)
                .WithMany(ws => ws.Workers)
                .HasForeignKey(u => u.WorkSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Presence>()
                .HasOne(dp => dp.Employee)
                .WithMany()
                .HasForeignKey(dp => dp.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Presence>()
                .HasOne(p => p.WorkSite)
                .WithMany(w => w.Presences)
                .HasForeignKey(p => p.WorkSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Purchase>()
                .HasOne(p => p.Material)
                .WithMany(m => m.Purchases)
                .HasForeignKey(p => p.MaterialId);

            builder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Purchase>()
                .HasOne(p => p.WorkSite)
                .WithMany(ws => ws.Purchases)
                .HasForeignKey(p => p.WorkSiteId);
        }
    }
}
