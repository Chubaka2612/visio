using Azure.Storage.Blobs;
using log4net;

namespace Visio.Data.Core.Storage
{
    public class StorageService : IStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(StorageService));

        public StorageService(StorageOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.ContainerId);
            ArgumentNullException.ThrowIfNull(options.BlobServiceClient);

            var blobServiceClient = options.BlobServiceClient;
            _containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerId);
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

                _logger.InfoFormat("File uploaded successfully: {FileName}", metadata.FileName);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error uploading file: {FileName}", metadata.FileName);
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

                _logger.InfoFormat("File downloaded successfully: {FileName}", fileName);
                return response.Value.Content;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error downloading file: {FileName}", fileName);
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
                    _logger.InfoFormat("Deleted blob: {BlobName}", blob.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Failed to delete all blobs.");
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

                _logger.InfoFormat("File deleted successfully: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error deleting file: {FileName}", fileName);
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

                _logger.InfoFormat("Retrieved {EntityCount} files from Azure Blob Storage.", fileUrls.Count);
                return fileUrls;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Failed to retrieve file list.");
                throw;
            }
        }
    }
}
