using Azure.Data.Tables;
using Azure;
using ST10443998_CLDV6212_POE.Models;

namespace ST10443998_CLDV6212_POE.Services
{
    
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

        public async Task<CustomerEntity?> GetAsync(string rowKey, CancellationToken ct = default)
        {
            try
            {
                var resp = await _table.GetEntityAsync<CustomerEntity>("Customer", rowKey, cancellationToken: ct);
                return resp.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404) { return null; }
        }

        public Task UpdateAsync(CustomerEntity entity, CancellationToken ct = default)
            => _table.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace, ct);

        public Task DeleteAsync(string rowKey, CancellationToken ct = default)
            => _table.DeleteEntityAsync("Customer", rowKey, cancellationToken: ct);
    }
}