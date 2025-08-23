using System.Collections.Generic;

namespace ST10443998_CLDV6212_POE.Models
{
    public class StorageDashboardVm
    {
        public List<CustomerEntity> Customers { get; set; } = new();
        public List<ProductEntity> Products { get; set; } = new();   
        public List<BlobItemVm> Blobs { get; set; } = new();
        public List<FileItemVm> Contracts { get; set; } = new();
        public List<QueueMessageVm> QueueMessages { get; set; } = new();
    }
}
