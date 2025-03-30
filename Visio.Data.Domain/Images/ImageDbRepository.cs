using Microsoft.Azure.Cosmos;
using System.Net;
using Visio.Data.Core.Db;
using Visio.Domain.Common.Images;

namespace Visio.Data.Domain.Images
{
    public class ImageDbRepository(RepositoryOptions options) : Repository<string, ImageEntity>("images-audit", options), IImageRepository
    {

        public async Task<IEnumerable<ImageEntity>> ReadAllAsync()
        {
            var query = "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(query);

            try
            {
                var images = await ReadAsync(queryDefinition);
                _logger.InfoFormat("Retrieved {ImageCount} images from database", images.Count());
                return images;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.WarnFormat("CosmosDB Container not found: {Message}", ex.Message);
                return Enumerable.Empty<ImageEntity>();
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error retrieving images from database");
                throw;
            }
        }

        public async Task<IEnumerable<ImageEntity>> GetImagesByLabelAsync(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                _logger.Warn("Label is empty, returning all images");
                return await ReadAllAsync();
            }

            var query = "SELECT * FROM c WHERE ARRAY_CONTAINS(c.labels, @label)";
            var queryDefinition = new QueryDefinition(query).WithParameter("@label", label);

            try
            {
                var images = await ReadAsync(queryDefinition);
                _logger.InfoFormat("Retrieved {ImageCount} images with label {Label}", images.Count(), label);
                return images;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error retrieving images with label {Label}", label);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetLabelsForImageAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.WarnFormat("Image ID is empty when fetching labels");
                return [];
            }

            var query = "SELECT c.labels FROM c WHERE c.id = @id";
            var queryDefinition = new QueryDefinition(query).WithParameter("@id", id);

            try
            {
                var images = await ReadAsync(queryDefinition);
                var labels = images.FirstOrDefault()?.Labels ?? [];
                _logger.InfoFormat("Retrieved {LabelCount} labels for image ID {ImageId}", labels.Count(), id);
                return labels;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error retrieving labels for image ID {ImageId}", id);
                throw;
            }
        }

        public async Task DeleteAllAsync()
        {
            _logger.WarnFormat("Deleting all images from database");
            var query = "SELECT c.id FROM c";
            var queryDefinition = new QueryDefinition(query);

            try
            {
                var images = await ReadAsync(queryDefinition);

                foreach (var image in images)
                {
                    if (!string.IsNullOrWhiteSpace(image.Id))
                    {
                        await DeleteAsync(image.Id);
                        _logger.InfoFormat("Deleted image with ID {ImageId}", image.Id);
                    }
                }
                _logger.InfoFormat("Successfully deleted all images");
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error deleting all images from database");
                throw;
            }
        }
    }
}