using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace Visio.Data.Core.Storage
{
    public class StorageService : IStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<StorageService> _logger;

        public StorageService(StorageOptions options, ILogger<StorageService> logger)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.ContainerId);
            ArgumentNullException.ThrowIfNull(options.BlobServiceClient);
            ArgumentNullException.ThrowIfNull(logger);

            var blobServiceClient = options.BlobServiceClient;
            _containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerId);
            _logger = logger;
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage using a raw stream and metadata.
        /// </summary>
        public async Task<string> CreateAsync(Stream fileStream, FileMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(fileStream);
            ArgumentNullException.ThrowIfNull(metadata);

            try
            {
                var blobClient = _containerClient.GetBlobClient(metadata.FileName);
                await blobClient.UploadAsync(fileStream, overwrite: true);

                _logger.LogInformation("File uploaded successfully: {FileName}", metadata.FileName);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", metadata.FileName);
                throw;
            }
        }

        /// <summary>
        /// Downloads a file from Azure Blob Storage.
        /// </summary>
        public async Task<Stream> ReadAsync(string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);

            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);
                var response = await blobClient.DownloadAsync();

                _logger.LogInformation("File downloaded successfully: {FileName}", fileName);
                return response.Value.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
                throw;
            }
        }

        /// <summary>
        /// Deletes all files from the storage container.
        /// </summary>
        public async Task DeleteAllAsync()
        {
            try
            {
                await foreach (var blob in _containerClient.GetBlobsAsync())
                {
                    await _containerClient.DeleteBlobAsync(blob.Name);
                    _logger.LogInformation("Deleted blob: {BlobName}", blob.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete all blobs.");
                throw;
            }
        }

        /// <summary>
        /// Deletes a file from Azure Blob Storage.
        /// </summary>
        public async Task DeleteAsync(string fileUrl)
        {
            ArgumentNullException.ThrowIfNull(fileUrl);

            var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);

            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();

                _logger.LogInformation("File deleted successfully: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the URLs of all files in the storage container.
        /// </summary>
        public async Task<IEnumerable<string>> ReadAllAsync()
        {
            var fileUrls = new List<string>();

            try
            {
                await foreach (var blob in _containerClient.GetBlobsAsync())
                {
                    var blobUrl = $"{_containerClient.Uri}/{blob.Name}";
                    fileUrls.Add(blobUrl);
                }

                _logger.LogInformation("Retrieved {EntityCount} files from Azure Blob Storage.", fileUrls.Count);
                return fileUrls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve file list.");
                throw;
            }
        }
    }
}
