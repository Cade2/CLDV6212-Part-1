using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Services;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class ContractsController : Controller
    {
        private readonly FileContractService _files;
        public ContractsController(FileContractService files) => _files = files;

        public async Task<IActionResult> Index()
            => View(await _files.ListAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file is null || file.Length == 0) { TempData["Err"] = "Choose a file."; return RedirectToAction(nameof(Index)); }
            await _files.UploadAsync(file);
            TempData["Ok"] = "Contract uploaded.";
            return RedirectToAction(nameof(Index));
        }
    }
}
