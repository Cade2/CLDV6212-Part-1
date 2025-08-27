using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Services;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class MediaController : Controller
    {
        private readonly BlobImageService _blobs;
        private readonly OrderQueueService _queue;

        // FIX: inject OrderQueueService since we use _queue in Delete()
        public MediaController(BlobImageService blobs, OrderQueueService queue)
        {
            _blobs = blobs;
            _queue = queue;
        }

        // NEW: q (search by name) + sort (name/date/size)
        public async Task<IActionResult> Index(string? q, string? sort)
        {
            var list = await _blobs.ListAsync();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                list = list.Where(b => b.Name.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            list = sort switch
            {
                "name_desc" => list.OrderByDescending(b => b.Name).ToList(),
                "date_asc" => list.OrderBy(b => b.LastModified ?? DateTimeOffset.MinValue).ToList(),
                "date_desc" => list.OrderByDescending(b => b.LastModified ?? DateTimeOffset.MinValue).ToList(),
                "size_asc" => list.OrderBy(b => b.Size ?? long.MaxValue).ToList(),
                "size_desc" => list.OrderByDescending(b => b.Size ?? long.MinValue).ToList(),
                _ => list.OrderBy(b => b.Name).ToList(), // name_asc default
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
                TempData["Err"] = "Choose an image.";
                return RedirectToAction(nameof(Index));
            }
            await _blobs.UploadImageAsync(file);
            TempData["Ok"] = "Image uploaded.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Err"] = "Missing blob name.";
                return RedirectToAction(nameof(Index));
            }
            await _blobs.DeleteAsync(name);
            await _queue.EnqueueAsync($"Deleted image \"{name}\"");
            TempData["Ok"] = "Image deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Download(string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest();

            var res = await _blobs.DownloadAsync(name, ct);
            if (res is null) return NotFound();

            return File(res.Value.Stream, res.Value.ContentType, res.Value.DownloadName);
        }

    }
}
