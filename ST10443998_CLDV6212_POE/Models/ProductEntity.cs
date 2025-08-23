using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ST10443998_CLDV6212_POE.Models
{
    // NEW: stores product info in Table Storage (meets rubric’s 'product-related information')
    public class ProductEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString("N"); // acts as Sku/ProductId
        [Required, MaxLength(120)] public string Title { get; set; } = string.Empty;
        [Range(0, 1_000_000)] public decimal Price { get; set; }
        public string? ImageBlobName { get; set; }   // optional: link to blob
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
