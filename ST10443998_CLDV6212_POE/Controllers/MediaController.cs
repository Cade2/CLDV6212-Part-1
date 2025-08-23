using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Services;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class MediaController : Controller
    {
        private readonly BlobImageService _blobs;
        public MediaController(BlobImageService blobs) => _blobs = blobs;

        public async Task<IActionResult> Index()
            => View(await _blobs.ListAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file is null || file.Length == 0) { TempData["Err"] = "Choose an image."; return RedirectToAction(nameof(Index)); }
            await _blobs.UploadImageAsync(file);
            TempData["Ok"] = "Image uploaded.";
            return RedirectToAction(nameof(Index));
        }
    }
}
