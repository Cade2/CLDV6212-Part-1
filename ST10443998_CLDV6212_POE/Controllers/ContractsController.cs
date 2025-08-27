using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Services;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class ContractsController : Controller
    {
        private readonly FileContractService _files;
        private readonly OrderQueueService _queue;

        // FIX: inject OrderQueueService since we use _queue in Delete()
        public ContractsController(FileContractService files, OrderQueueService queue)
        {
            _files = files;
            _queue = queue;
        }

        // NEW: q (search by name) + sort (name/date/size)
        public async Task<IActionResult> Index(string? q, string? sort)
        {
            var list = await _files.ListAsync();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                list = list.Where(f => f.Name.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            list = sort switch
            {
                "name_desc" => list.OrderByDescending(f => f.Name).ToList(),
                "date_asc" => list.OrderBy(f => f.LastModified ?? DateTimeOffset.MinValue).ToList(),
                "date_desc" => list.OrderByDescending(f => f.LastModified ?? DateTimeOffset.MinValue).ToList(),
                "size_asc" => list.OrderBy(f => f.Size ?? long.MaxValue).ToList(),
                "size_desc" => list.OrderByDescending(f => f.Size ?? long.MinValue).ToList(),
                _ => list.OrderBy(f => f.Name).ToList(), // name_asc default
            };

            ViewBag.Q = q;
            ViewBag.Sort = sort;
            return View(list);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                TempData["Err"] = "Choose a file.";
                return RedirectToAction(nameof(Index));
            }
            await _files.UploadAsync(file);
            TempData["Ok"] = "Contract uploaded.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Err"] = "Missing file name.";
                return RedirectToAction(nameof(Index));
            }
            await _files.DeleteAsync(name);
            await _queue.EnqueueAsync($"Deleted contract \"{name}\"");
            TempData["Ok"] = "Contract deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Download(string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest();

            var res = await _files.DownloadAsync(name, ct);
            if (res is null || res.Value.Stream is null) return NotFound();

            return File(res.Value.Stream, res.Value.ContentType, name);
        }

    }
}
