using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Models;
using ST10443998_CLDV6212_POE.Services;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductTableService _products;
        private readonly OrderQueueService _queue; // optional: log actions

        public ProductsController(ProductTableService products, OrderQueueService queue)
        {
            _products = products; _queue = queue;
        }

        public async Task<IActionResult> Index()
            => View(await _products.ListAsync(500));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, decimal price)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Err"] = "Title is required.";
                return RedirectToAction(nameof(Index));
            }
            await _products.AddAsync(new ProductEntity { Title = title.Trim(), Price = price });
            await _queue.EnqueueAsync($"Added product \"{title}\" for {price:C}");
            TempData["Ok"] = "Product saved.";
            return RedirectToAction(nameof(Index));
        }
    }
}
