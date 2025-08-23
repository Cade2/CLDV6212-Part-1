using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Services;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class QueueController : Controller
    {
        private readonly OrderQueueService _queue;
        public QueueController(OrderQueueService queue) => _queue = queue;

        public async Task<IActionResult> Index()
            => View(await _queue.PeekAsync(32));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Enqueue(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) { TempData["Err"] = "Enter a message."; return RedirectToAction(nameof(Index)); }
            await _queue.EnqueueAsync(text.Trim());
            TempData["Ok"] = "Message queued.";
            return RedirectToAction(nameof(Index));
        }
    }
}
