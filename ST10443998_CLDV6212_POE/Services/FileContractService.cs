using Azure;
using Azure.Storage.Files.Shares;
using ST10443998_CLDV6212_POE.Models;

namespace ST10443998_CLDV6212_POE.Services
{
    public class FileContractService
    {
        private readonly ShareClient _share;
        public FileContractService(ShareClient share) => _share = share;
        public async Task UploadAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file provided");
            await _share.CreateIfNotExistsAsync(cancellationToken: ct);
            var root = _share.GetRootDirectoryClient();
            var fileClient = root.GetFileClient(Path.GetFileName(file.FileName));
            await fileClient.CreateAsync(file.Length, cancellationToken: ct);
            await using var stream = file.OpenReadStream();
            await fileClient.UploadRangeAsync(new HttpRange(0, file.Length), stream, cancellationToken: ct);
        }

        public async Task<List<FileItemVm>> ListAsync(CancellationToken ct = default)
        {
            await _share.CreateIfNotExistsAsync(cancellationToken: ct);

            var root = _share.GetRootDirectoryClient();
            var items = new List<FileItemVm>();

            await foreach (var entry in root.GetFilesAndDirectoriesAsync(cancellationToken: ct))
            {
                if (entry.IsDirectory) continue;

                var file = root.GetFileClient(entry.Name);
                var props = await file.GetPropertiesAsync(cancellationToken: ct);

                items.Add(new FileItemVm(
                    Name: entry.Name,
                    Size: props.Value.ContentLength,
                    LastModified: props.Value.LastModified
                ));
            }

            return items;
        }
    }
}