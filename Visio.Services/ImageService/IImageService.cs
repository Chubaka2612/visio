using Visio.Data.Core;
using Visio.Domain.Common.Images;

namespace Visio.Services.ImageService
{
    public interface IImageService
    {
        Task<string> CreateAsync(Stream fileStream, FileMetadata metadata);

        Task<Stream> GetAsync(string id);

        Task DeleteAsync(string id);

        Task DeleteAllAsync();

        Task<IEnumerable<ImageEntity>> SearchAsync(string label);

    }
}
