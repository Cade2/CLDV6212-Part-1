namespace ST10443998_CLDV6212_POE.Models
{
    // Represents the JSON payload you put into the queue
    public record OrderEventVm(string? OrderId, string? Description, DateTimeOffset? CreatedUtc);
}
