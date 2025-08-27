using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Models;
using ST10443998_CLDV6212_POE.Services;
using System.Text.Json;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class QueueController : Controller
    {
        private readonly OrderQueueService _queue;
        public QueueController(OrderQueueService queue) => _queue = queue;

        public async Task<IActionResult> Index(string? q)
        {
            var list = await _queue.PeekAsync(32);

            // optional filtering first
            if (!string.IsNullOrWhiteSpace(q))
                list = list.Where(m => (m.Text ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            // parse JSON -> QueuePrettyVm
            var pretty = new List<QueuePrettyVm>();
            foreach (var m in list)
            {
                OrderEventVm? evt = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(m.Text))
                    {
                        using var doc = JsonDocument.Parse(m.Text);
                        var root = doc.RootElement;

                        string? orderId = root.TryGetProperty("OrderId", out var v1) ? v1.GetString() : null;
                        string? description = root.TryGetProperty("Description", out var v2) ? v2.GetString() : null;

                        DateTimeOffset? createdUtc = null;
                        if (root.TryGetProperty("CreatedUtc", out var v3))
                        {
                            // handle ISO string or DateTime value
                            if (v3.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(v3.GetString(), out var p))
                                createdUtc = p;
                            else if (v3.ValueKind == JsonValueKind.Number && v3.TryGetInt64(out var epoch))
                                createdUtc = DateTimeOffset.FromUnixTimeSeconds(epoch);
                        }

                        if (orderId != null || description != null || createdUtc != null)
                            evt = new OrderEventVm(orderId, description, createdUtc);
                    }
                }
                catch { /* leave evt = null; fall back to raw */ }

                pretty.Add(new QueuePrettyVm
                {
                    Id = m.Id,
                    RawText = m.Text,
                    InsertedOn = m.InsertedOn,
                    Event = evt
                });
            }

            // newest first
            pretty = pretty.OrderByDescending(x => x.InsertedOn ?? DateTimeOffset.MinValue).ToList();

            ViewBag.Q = q;
            return View(pretty);
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
