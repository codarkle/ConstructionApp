using ConstructionApp.Data;
using ConstructionApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Identity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace ConstructionApp.Controllers
{
    public class MaterialsController : AppController
    {
        public MaterialsController(UserManager<User> userManager, AppDbContext context)
            : base(userManager, context)
        {

        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Materials/Create
        [Authorize(Roles = "Admin,Surveyor,Manager")]
        public IActionResult Create()
        {
            return PartialView("_Create", new Material());
        }

        // POST: /Materials/Create
        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create(Material material)
        {
            if (ModelState.IsValid)
            {
                _context.Add(material);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return PartialView("_Create", material);
        }

        // GET: /Materials/Edit/5
        [Authorize(Roles = "Admin,Surveyor,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
                return NotFound();

            return PartialView("_Edit", material);
        }

        // POST: /Materials/Edit
        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        public async Task<IActionResult> Edit(Material material)
        {
            if (ModelState.IsValid)
            {
                _context.Update(material);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return PartialView("_Edit", material);
        }

        // POST: /Materials/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
                return NotFound();

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        public JsonResult GetMaterialList()
        {
            int totalRecords = 0;
            int filteredRecords = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            var data = _context.Materials.AsQueryable();

            totalRecords = data.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                data = data.Where(m => m.Name.ToLower().Contains(lower) ||
                                   (m.Description != null && m.Description.ToLower().Contains(lower)));
            }

            filteredRecords = data.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection)
                && !string.Equals(sortColumn, "No", StringComparison.OrdinalIgnoreCase))
            {
                data = data.OrderBy($"{sortColumn} {sortDirection}");
            }

            var pagedData = data.Skip(skip).Take(pageSize).ToList();
            var materialList = pagedData
                .Select((m, index) => new
                {
                    No = skip + index + 1,
                    id = m.Id,
                    name = m.Name,
                    description = m.Description
                }).ToList();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = materialList
            });
        }

    }
}
