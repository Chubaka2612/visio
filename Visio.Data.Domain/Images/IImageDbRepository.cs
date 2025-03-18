using Visio.Data.Core.Db;
using Visio.Domain.Common.Images;

namespace Visio.Data.Domain.Images
{
    public interface IImageRepository : IDbRepository<string, ImageEntity>
    {
        Task<IEnumerable<ImageEntity>> GetImagesByLabelAsync(string label);

        Task<IEnumerable<string>> GetLabelsForImageAsync(string id);

        Task<IEnumerable<ImageEntity>> ReadAllAsync();

        Task DeleteAllAsync();
    }
}
