using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ST10443998_CLDV6212_POE.Models
{
    public class CustomerEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get; set; } = Guid.NewGuid().ToString("N"); 
        [Required, MaxLength(60)] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(60)] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
