using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ST10443998_CLDV6212_POE.Models
{
    public class ProductEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString("N"); 

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Range(0, 1_000_000)]
        public double Price { get; set; } = 0d;

        public string? ImageBlobName { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
