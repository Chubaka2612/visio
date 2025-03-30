using Visio.Data.Core;
using Visio.Domain.Common.Images;

namespace Visio.Services.ImageService
{
    public interface IImageService
    {
        Task<string> CreateAsync(Stream fileStream, FileMetadata metadata);

        Task<ConsolidatedImage> GetAsync(string id);

        Task DeleteAsync(string id);

        Task DeleteAllAsync();

        Task<IEnumerable<ConsolidatedImage>> SearchAsync(string label);

        Task UpdateImageLabelsAsync(string imageId, List<string> labels);

        Task<IEnumerable<ConsolidatedImage>> GetAllImagesAsync();
    }
}
