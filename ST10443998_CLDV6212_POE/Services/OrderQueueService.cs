using Azure.Storage.Queues;
using ST10443998_CLDV6212_POE.Models;
using System.Linq;
using System.Text.Json;

namespace ST10443998_CLDV6212_POE.Services
{
    public class OrderQueueService
    {
        private readonly QueueClient _queue;
        public OrderQueueService(QueueClient queue)
        {
            _queue = queue;
            _queue.CreateIfNotExistsAsync();
        }
        public Task EnqueueAsync(string description, CancellationToken ct = default)
        {
            var payload = JsonSerializer.Serialize(new
            {
                OrderId = Guid.NewGuid().ToString(),
                Description = description,
                CreatedUtc = DateTime.UtcNow,
            });
            return _queue.SendMessageAsync(payload, ct);
        }

        public async Task<List<QueueMessageVm>> PeekAsync(int count = 16, CancellationToken ct = default)
        {
            if (count < 1) count = 1;
            if (count > 32) count = 32; // hard cap per Azure Queues API

            var peeked = await _queue.PeekMessagesAsync(count, ct);
            return peeked.Value.Select(m => new QueueMessageVm(
                Id: m.MessageId,
                Text: m.Body?.ToString(),
                InsertedOn: m.InsertedOn
            )).ToList();
        }

        public async Task<int> DequeueAndDeleteAsync(int count = 10, CancellationToken ct = default)
        {
            if (count < 1) count = 1;
            if (count > 32) count = 32;

            var received = await _queue.ReceiveMessagesAsync(count, cancellationToken: ct);
            int deleted = 0;
            foreach (var m in received.Value)
            {
                await _queue.DeleteMessageAsync(m.MessageId, m.PopReceipt, ct);
                deleted++;
            }
            return deleted;
        }

        public Task ClearAsync(CancellationToken ct = default)
            => _queue.ClearMessagesAsync(ct);

    }
}