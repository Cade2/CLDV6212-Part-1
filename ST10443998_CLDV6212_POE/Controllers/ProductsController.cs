using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Models;
using ST10443998_CLDV6212_POE.Services;
using System.Globalization;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductTableService _products;
        private readonly OrderQueueService _queue;
        public ProductsController(ProductTableService products, OrderQueueService queue) { _products = products; _queue = queue; }

        public async Task<IActionResult> Index(string? sort, string? q, double? min, double? max)
        {
            var list = await _products.ListAsync(1000);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                list = list.Where(p => p.Title.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (min.HasValue) list = list.Where(p => p.Price >= min.Value).ToList();
            if (max.HasValue) list = list.Where(p => p.Price <= max.Value).ToList();

            list = sort switch
            {
                "title_desc" => list.OrderByDescending(p => p.Title).ToList(),
                "price_asc" => list.OrderBy(p => p.Price).ToList(),
                "price_desc" => list.OrderByDescending(p => p.Price).ToList(),
                _ => list.OrderBy(p => p.Title).ToList(),
            };

            ViewBag.Sort = sort;
            ViewBag.Q = q;
            ViewBag.Min = min;
            ViewBag.Max = max;
            return View(list);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string price)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Err"] = "Title is required.";
                return RedirectToAction(nameof(Index));
            }

            if (!decimal.TryParse(price, NumberStyles.Any, CultureInfo.CurrentCulture, out var priceDec))
            {
                TempData["Err"] = "Invalid price format.";
                return RedirectToAction(nameof(Index));
            }

            var p = new ProductEntity { Title = title.Trim(), Price = (double)priceDec };
            await _products.AddAsync(p);
            await _queue.EnqueueAsync($"Added product \"{p.Title}\" for {p.Price.ToString("C", CultureInfo.CurrentCulture)}");
            TempData["Ok"] = "Product saved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var p = await _products.GetAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEntity model)
        {
            if (!ModelState.IsValid) return View(model);
            var existing = await _products.GetAsync(model.RowKey);
            if (existing is null) return NotFound();

            existing.Title = model.Title?.Trim() ?? "";
            existing.Price = model.Price;   

            await _products.UpdateAsync(existing);
            TempData["Ok"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) { TempData["Err"] = "Missing id."; return RedirectToAction(nameof(Index)); }
            await _products.DeleteAsync(id);
            TempData["Ok"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
