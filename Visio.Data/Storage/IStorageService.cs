namespace Visio.Data.Core.Storage
{
    public interface IStorageService
    {
        Task<string> CreateAsync(Stream fileStream, FileMetadata metadata);

        Task<Stream> ReadAsync(string fileName);

        Task<IEnumerable<string>> ReadAllAsync();

        Task DeleteAsync(string fileUrl);

        Task DeleteAllAsync();
    }
}