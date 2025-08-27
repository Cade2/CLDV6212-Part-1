using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Models;
using ST10443998_CLDV6212_POE.Services;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class CustomersController : Controller
    {

        private readonly CustomerTableService _tables;
        private readonly OrderQueueService _queue;
        public CustomersController(CustomerTableService tables, OrderQueueService queue) { _tables = tables; _queue = queue; }

        public async Task<IActionResult> Index(string? sort, string? q)
        {
            var list = await _tables.ListAsync(1000);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                list = list.Where(c =>
                    (c.FirstName + " " + c.LastName).Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    c.Email.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            list = sort switch
            {
                "lname_asc" => list.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList(),
                "lname_desc" => list.OrderByDescending(c => c.LastName).ThenByDescending(c => c.FirstName).ToList(),
                "name_desc" => list.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName).ToList(),
                "email_asc" => list.OrderBy(c => c.Email).ToList(),
                "email_desc" => list.OrderByDescending(c => c.Email).ToList(),
                _ => list.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToList(),
            };

            ViewBag.Sort = sort;
            ViewBag.Q = q;
            return View(list);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string firstName, string lastName, string email)
        {
            if (string.IsNullOrWhiteSpace(email)) { TempData["Err"] = "Email is required."; return RedirectToAction(nameof(Index)); }
            var entity = new CustomerEntity { FirstName = firstName?.Trim() ?? "", LastName = lastName?.Trim() ?? "", Email = email.Trim() };
            await _tables.AddCustomerAsync(entity);
            await _queue.EnqueueAsync($"Added customer \"{entity.FirstName} {entity.LastName}\" <{entity.Email}>");
            TempData["Ok"] = "Customer saved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var c = await _tables.GetAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerEntity model)
        {
            if (!ModelState.IsValid) return View(model);
            await _tables.UpdateAsync(model);
            TempData["Ok"] = "Customer updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) { TempData["Err"] = "Missing id."; return RedirectToAction(nameof(Index)); }
            await _tables.DeleteAsync(id);
            TempData["Ok"] = "Customer deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
