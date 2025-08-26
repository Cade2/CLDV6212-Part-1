using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Services;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class QueueController : Controller
    {
        private readonly OrderQueueService _queue;
        public QueueController(OrderQueueService queue) => _queue = queue;

        public async Task<IActionResult> Index()
        {
            var list = await _queue.PeekAsync(32);  // <= 32
            list = list.OrderByDescending(m => m.InsertedOn ?? DateTimeOffset.MinValue).ToList();
            return View(list);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Enqueue(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) { TempData["Err"] = "Enter a message."; return RedirectToAction(nameof(Index)); }
            await _queue.EnqueueAsync(text.Trim());
            TempData["Ok"] = "Message queued.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Dequeue(int n = 10)
        {
            var deleted = await _queue.DequeueAndDeleteAsync(n);
            TempData["Ok"] = $"Dequeued & deleted {deleted} message(s).";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            await _queue.ClearAsync();
            TempData["Ok"] = "Queue cleared.";
            return RedirectToAction(nameof(Index));
        }

    }
}
