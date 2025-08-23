using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ST10443998_CLDV6212_POE.Models;

namespace ST10443998_CLDV6212_POE.Services
{
    public class BlobImageService
    {
        private readonly BlobContainerClient _container;
        public BlobImageService(BlobContainerClient container) => _container = container;
        public async Task<string> UploadImageAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file provided");
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only image files are allowed");
            await _container.CreateIfNotExistsAsync(PublicAccessType.BlobContainer, cancellationToken: ct);
            var blobName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var blob = _container.GetBlobClient(blobName);
            var headers = new BlobHttpHeaders { ContentType = file.ContentType };
            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers }, ct);
            return blob.Uri.ToString();
        }

        public async Task<List<BlobItemVm>> ListAsync(CancellationToken ct = default)
        {
            await _container.CreateIfNotExistsAsync(PublicAccessType.BlobContainer, cancellationToken: ct);

            var items = new List<BlobItemVm>();
            await foreach (var b in _container.GetBlobsAsync(cancellationToken: ct))
            {
                var url = _container.GetBlobClient(b.Name).Uri.ToString();
                items.Add(new BlobItemVm(
                    Name: b.Name,
                    Size: b.Properties.ContentLength,
                    LastModified: b.Properties.LastModified,
                    Url: url
                ));
            }
            return items;
        }
    }
}
