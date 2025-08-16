using ConstructionApp.Models;
using ConstructionApp.ViewModels;
using ConstructionApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Linq.Dynamic.Core;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ConstructionApp.Helpers;


namespace ConstructionApp.Controllers
{
    public class PurchasesController : AppController
    {

        public PurchasesController(UserManager<User> userManager, AppDbContext context)
            : base(userManager, context)
        {

        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpGet]
        public async Task<IActionResult> Create(int workSiteId)
        {
            var viewModel = new PurchaseViewModel
            {
                WorkSiteId = workSiteId,
                Materials = new SelectList(await _context.Materials.ToListAsync(), "Id", "Name"),
                Suppliers = new SelectList(await _context.Users.Where(u => u.RoleId == RoleIds.Supplier).ToListAsync(), "Id", "UserName"),   
                DateDoc = DateTime.Today,
            };
            return PartialView("_Create", viewModel);
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Materials = new SelectList(await _context.Materials.ToListAsync(), "Id", "Name");
                model.Suppliers = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName");
                return PartialView("_Create", model);
            }

            var purchase = new Purchase
            {
                WorkSiteId = model.WorkSiteId,
                MaterialId = model.MaterialId,
                SupplierId = model.SupplierId,
                Quantity = model.Quantity,
                Amount = model.Amount,
                DateDoc = model.DateDoc,
                DocNumber = model.DocNumber
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Purchases"); // AJAX expects JSON here
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        public JsonResult GetPurchaseList()
        {
            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            // Base query
            var data = _context.Purchases
                .Include(p => p.Material)
                .Include(p => p.Supplier)
                .Include(p => p.WorkSite)
                .AsQueryable();
            //get total count of data in table
            totalRecord = data.Count();
            // search data when search value found

            if (IsTopLeader)
            {
                string? workSiteIdStr = Request.Form["workSiteDropDown"].FirstOrDefault();
                int? workSiteId = string.IsNullOrWhiteSpace(workSiteIdStr) ? null : int.Parse(workSiteIdStr);
                if (workSiteId != 0)
                {  
                    data = data.Where(x => x.WorkSiteId == workSiteId);
                }
            }
            else if (IsManager)
            {
                data = data.Where(
                    x => x.WorkSiteId == MyWorkSiteId);
            }
            else
            {
                return Json(null);
            } 

            if (!string.IsNullOrEmpty(searchValue))
            {
                var lowerSearch = searchValue.ToLower();

                data = data.Where(x =>
                    x.DocNumber != null && x.DocNumber.ToLower().Contains(lowerSearch) ||
                    x.Material.Name != null && x.Material.Name.ToLower().Contains(lowerSearch) ||
                    x.Supplier.UserName != null && x.Supplier.UserName.ToLower().Contains(lowerSearch) ||
                    x.WorkSite.Name != null && x.WorkSite.Name.ToLower().Contains(lowerSearch));
            }
            // get total count of records after search
            filterRecord = data.Count();
            //sort data
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection)
                && !string.Equals(sortColumn, "No", StringComparison.OrdinalIgnoreCase))
            {
                data = data.OrderBy($"{sortColumn} {sortColumnDirection}");
            }
            var totalCount = data.Sum(p => p.Quantity);
            var totalSum = data.Sum(p => p.Amount * p.Quantity);
            //pagination
            var pagedData = data.Skip(skip).Take(pageSize).ToList();

            var empList = pagedData
              .Select((p, index) => new {
                  no = skip + index + 1,
                  id = p.Id,
                  workSiteName = p.WorkSite.Name,
                  materialName = p.Material.Name,
                  supplierName = p.Supplier.UserName,
                  quantity = p.Quantity,
                  amount = CurrencyHelper.FormatEuro(p.Amount),
                  dateDoc = p.DateDoc.ToString("dd/MM/yyyy"),
                  docNumber = p.DocNumber,
              }).ToList();


            var returnObj = new
            {
                draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = empList,
                count = totalCount,
                sum = totalSum
            };

            return Json(returnObj);
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Material)
                .Include(p => p.Supplier)
                .Include(p => p.WorkSite)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound();
            }

            var viewModel = new PurchaseViewModel
            {
                Id = purchase.Id,
                DocNumber = purchase.DocNumber,
                DateDoc = purchase.DateDoc,
                MaterialId = purchase.MaterialId,
                SupplierId = purchase.SupplierId,
                WorkSiteId = purchase.WorkSiteId,
                Quantity = purchase.Quantity,
                Amount = purchase.Amount,
                Materials = new SelectList(await _context.Materials.ToListAsync(), "Id", "Name", purchase.MaterialId),
                Suppliers = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName", purchase.SupplierId),
                WorkSites = new SelectList(await _context.WorkSites.ToListAsync(), "Id", "Name", purchase.WorkSiteId)
            };

            return PartialView("_Edit", viewModel);
        }

        [Authorize(Roles = "Admin,Surveyor,Manager")]
        [HttpPost]
        public async Task<IActionResult> Edit(PurchaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Materials = new SelectList(await _context.Materials.ToListAsync(), "Id", "Name", model.MaterialId);
                model.Suppliers = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName", model.SupplierId);
                model.WorkSites = new SelectList(await _context.WorkSites.ToListAsync(), "Id", "Name", model.WorkSiteId);
                return PartialView("_Edit", model);
            }

            var purchase = await _context.Purchases.FindAsync(model.Id);
            if (purchase == null)
                return NotFound();

            purchase.DocNumber = model.DocNumber;
            purchase.DateDoc = model.DateDoc;
            purchase.MaterialId = model.MaterialId;
            purchase.SupplierId = model.SupplierId;
            purchase.WorkSiteId = model.WorkSiteId;
            purchase.Quantity = model.Quantity;
            purchase.Amount = model.Amount;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Purchases");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
                return NotFound();

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Purchases"); 
        }
    }
}
