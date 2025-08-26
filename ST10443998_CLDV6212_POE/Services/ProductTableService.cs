using Azure.Data.Tables;
using ST10443998_CLDV6212_POE.Models;

namespace ST10443998_CLDV6212_POE.Services
{
    public class ProductTableService
    {
        private readonly TableClient _table;
        public ProductTableService(TableClient table)   // register this client to your "Products" table in Program.cs
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
    }
}
