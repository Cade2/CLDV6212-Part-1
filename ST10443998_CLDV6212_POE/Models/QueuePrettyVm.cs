using System;

namespace ST10443998_CLDV6212_POE.Models
{
    // Wraps the raw queue message + parsed fields (if JSON)
    public class QueuePrettyVm
    {
        public string Id { get; init; } = "";
        public string? RawText { get; init; }
        public DateTimeOffset? InsertedOn { get; init; }
        public OrderEventVm? Event { get; init; } // null if not JSON / missing keys
    }
}
