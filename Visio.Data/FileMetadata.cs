
namespace Visio.Data.Core
{
    public class FileMetadata
    {
        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = "application/octet-stream"; // Default

        public long Size { get; set; }
    }
}
