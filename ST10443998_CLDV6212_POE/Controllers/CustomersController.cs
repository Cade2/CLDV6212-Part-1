using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Models;
using ST10443998_CLDV6212_POE.Services;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class CustomersController : Controller
    {

        private readonly CustomerTableService _tables;
        public CustomersController(CustomerTableService tables) => _tables = tables;

        public async Task<IActionResult> Index()
            => View(await _tables.ListAsync(500));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string firstName, string lastName, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Err"] = "Email is required.";
                return RedirectToAction(nameof(Index));
            }
            await _tables.AddCustomerAsync(new CustomerEntity
            {
                FirstName = firstName?.Trim() ?? "",
                LastName = lastName?.Trim() ?? "",
                Email = email.Trim()
            });
            TempData["Ok"] = "Customer saved.";
            return RedirectToAction(nameof(Index));
        }
    }
}
