using Azure;
using Azure.Data.Tables;
using ST10443998_CLDV6212_POE.Models;

namespace ST10443998_CLDV6212_POE.Services
{
    public class ProductTableService
    {
        private readonly TableClient _table;
        public ProductTableService(TableClient table)   
        {
            _table = table;
            _table.CreateIfNotExists();
        }

        public Task AddAsync(ProductEntity entity, CancellationToken ct = default)
        => _table.AddEntityAsync(entity, cancellationToken: ct);

        public async Task<List<ProductEntity>> ListAsync(int take = 500, CancellationToken ct = default)
        {
            var results = new List<ProductEntity>();
            await foreach (var e in _table.QueryAsync<ProductEntity>(cancellationToken: ct))
            {
                results.Add(e);
                if (results.Count >= take) break;
            }
            return results;
        }

        public async Task<ProductEntity?> GetAsync(string rowKey, CancellationToken ct = default)
        {
            try
            {
                var resp = await _table.GetEntityAsync<ProductEntity>("Product", rowKey, cancellationToken: ct);
                return resp.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404) { return null; }
        }

        public Task UpdateAsync(ProductEntity entity, CancellationToken ct = default)
            => _table.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace, ct);

        public Task DeleteAsync(string rowKey, CancellationToken ct = default)
            => _table.DeleteEntityAsync("Product", rowKey, cancellationToken: ct);
    }
}
