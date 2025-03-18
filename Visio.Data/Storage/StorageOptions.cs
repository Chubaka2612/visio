using Azure.Storage.Blobs;

namespace Visio.Data.Core.Storage
{
    public class StorageOptions
    {
        public string ContainerId { get; set; }

        public BlobServiceClient BlobServiceClient { get; set; }

        public StorageOptions()
        {
        }

        public StorageOptions(string containerId, BlobServiceClient blobClient)
        {
            ContainerId = containerId;
            BlobServiceClient = blobClient;
        }
    }
}
