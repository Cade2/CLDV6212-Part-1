using Azure.Data.Tables;
using Azure;
using ST10443998_CLDV6212_POE.Models;

namespace ST10443998_CLDV6212_POE.Services
{

    //public class CustomerEntity : ITableEntity
    //{
    //    public string PartitionKey { get; set; } = "Customer";
    //    public string RowKey { get; set; } = Guid.NewGuid().ToString("N");
    //    public string FirstName { get; set; } = string.Empty;
    //    public string LastName { get; set; } = string.Empty;
    //    public string Email { get; set; } = string.Empty;
    //    public ETag ETag { get; set; }
    //    public DateTimeOffset? Timestamp { get; set; }
    //}

    public class CustomerTableService
    {
        private readonly TableClient _table;
        public CustomerTableService(TableClient table)
        {
            _table = table;
            _table.CreateIfNotExists();
        }
        public Task AddCustomerAsync(CustomerEntity entity, CancellationToken ct = default)
        => _table.AddEntityAsync(entity, cancellationToken: ct);

        public async Task<List<CustomerEntity>> ListAsync(int take = 500, CancellationToken ct = default)
        {
            var results = new List<CustomerEntity>();
            await foreach (var e in _table.QueryAsync<CustomerEntity>(cancellationToken: ct))
            {
                results.Add(e);
                if (results.Count >= take) break;
            }
            return results;
        }
    }
}