using Azure;
using log4net;
using Visio.Data.Core;
using Visio.Data.Core.Storage;
using Visio.Data.Domain.Images;
using Visio.Domain.Common;
using Visio.Domain.Common.Images;
using Visio.Services.Notifications;

namespace Visio.Services.ImageService
{
    public class ImageService : IImageService
    {
        private readonly IStorageService _storageService;
        private readonly IImageRepository _imageRepository;
        private readonly INotificationProducer _notificationProducer;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ImageService));

        public ImageService(IStorageService storageService, IImageRepository imageRepository, INotificationProducer notificationProducer)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _notificationProducer = notificationProducer ?? throw new ArgumentNullException(nameof(notificationProducer));
        }

        public ImageService(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        public async Task<string> CreateAsync(Stream fileStream, FileMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(fileStream);
            ArgumentNullException.ThrowIfNull(metadata);

            try
            {
                // Upload file to Azure Blob Storage
                string fileUrl = await _storageService.CreateAsync(fileStream, metadata);

                // Save metadata in CosmosDB
                var imageEntity = new ImageEntity
                {
                    ObjectPath = fileUrl,
                    ObjectSize = metadata.Size.ToString(),
                    Status = ImageStatus.New.ToString()
                };

                await _imageRepository.CreateAsync(imageEntity);

                _logger.InfoFormat("Image successfully created with ID: {Id}", metadata.FileName);

                //Trigger notification
                var notification = BuildNotification(imageEntity);

                await _notificationProducer.PublishMessageAsync(notification, ((NotificationProducer)_notificationProducer).Options.QueueName);

                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error creating image");
                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            try
            {
                // Get image metadata from DB
                var image = await _imageRepository.ReadAsync(id);
                if (image == null)
                {
                    _logger.WarnFormat("Image with ID {Id} not found", id);
                    return;
                }

                // Delete from Blob Storage
                await _storageService.DeleteAsync(image.ObjectPath);

                // Delete metadata from CosmosDB
                await _imageRepository.DeleteAsync(id);

                _logger.InfoFormat("Successfully deleted image with ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error deleting image with ID {Id}", id);
                throw;
            }
        }

        public async Task<ConsolidatedImage> GetAsync(string id)
        {
            try
            {
                _logger.InfoFormat("Fetching image with ID: {0}", id);

                // Retrieve image metadata from CosmosDB
                var image = await _imageRepository.ReadAsync(id);
                if (image == null)
                {
                    _logger.WarnFormat("No image found with ID: {0}", id);
                    return null;
                }

                // Fetch image stream from Blob Storage
                string fileName = Path.GetFileName(image.ObjectPath);
                var imageStream = await _storageService.ReadAsync(fileName);

                if (imageStream == null)
                {
                    _logger.WarnFormat("Could not retrieve image from storage: {0}", image.ObjectPath);
                    return null;
                }

                _logger.InfoFormat("Successfully retrieved image with ID: {0}", id);
                return new ConsolidatedImage
                {
                    Stream = imageStream,
                    ImageEntity = image
                };
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error occurred while fetching image with ID: {0}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ConsolidatedImage>> GetAllImagesAsync()
        {
            //TODO: think about limit amount
            try
            {
                _logger.Info("Fetching all images and labels.");

                // Retrieve all image metadata from CosmosDB
                var images = await _imageRepository.ReadAllAsync();
                if (images == null || !images.Any())
                {
                    _logger.Warn("No images found.");
                    return Enumerable.Empty<ConsolidatedImage>();
                }

                var result = new List<ConsolidatedImage>();

                foreach (var image in images)
                {
                    try
                    {
                        // Fetch image stream from Blob Storage
                        string fileName = Path.GetFileName(image.ObjectPath);
                        var imageStream = await _storageService.ReadAsync(fileName);

                        if (imageStream == null)
                        {
                            _logger.WarnFormat("Could not retrieve image from storage: {0}", image.ObjectPath);
                            continue;
                        }

                        result.Add(new ConsolidatedImage
                        {
                            Stream = imageStream,
                            ImageEntity = image
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorFormat(ex.Message, "Error retrieving image from storage: {0}", image.ObjectPath);
                    }
                }

                _logger.InfoFormat("Successfully retrieved {0} images.", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error occurred while fetching all images.");
                throw;
            }
        }

        public async Task DeleteAllAsync()
        {
            try
            {
                _logger.WarnFormat("Starting deletion of all images from storage and database.");

                // Delete all images from CosmosDB
                await _imageRepository.DeleteAllAsync();
                _logger.InfoFormat("Deleted all images from database.");

                // Delete all files from Blob Storage
                await _storageService.DeleteAllAsync();
                _logger.InfoFormat("Deleted all files from blob storage.");

                _logger.InfoFormat("Successfully deleted all images.");
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error occurred while deleting all images.");
                throw;
            }
        }

        public async Task<IEnumerable<ConsolidatedImage>> SearchAsync(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentNullException(nameof(label));
            }

            try
            {
                _logger.InfoFormat("Searching for images with label: {0}", label);

                // Retrieve all images from the repository by label
                var images = await _imageRepository.GetImagesByLabelAsync(label);
                if (images == null || !images.Any())
                {
                    _logger.WarnFormat("No images found for label: {0}", label);
                    return Enumerable.Empty<ConsolidatedImage>();
                }

                var result = new List<ConsolidatedImage>();

                foreach (var image in images)
                {
                    try
                    {
                        // Fetch image stream from Blob Storage
                        string fileName = Path.GetFileName(image.ObjectPath);
                        var imageStream = await _storageService.ReadAsync(fileName);

                        if (imageStream == null)
                        {
                            _logger.WarnFormat("Could not retrieve image from storage: {0}", image.ObjectPath);
                            continue;
                        }

                        result.Add(new ConsolidatedImage
                        {
                            Stream = imageStream,
                            ImageEntity = image
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorFormat(ex.Message, "Error retrieving image from storage: {0}", image.ObjectPath);
                    }
                }

                _logger.InfoFormat("Successfully retrieved {0} images for label: {1}", result.Count, label);
                return result;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "Error occurred while searching for images with label {0}", label);
                throw;
            }
        }

        public async Task UpdateImageLabelsAsync(string imageId, List<string> labels)
        {
            var imageEntity = await _imageRepository.ReadAsync(imageId);
            if (imageEntity != null)
            {
                imageEntity.Labels = labels;
                imageEntity.Status = ImageStatus.RecognitionCompleted.ToString();

                await _imageRepository.UpdateAsync(imageEntity);
                _logger.InfoFormat("Updated Image with ID {Id} with labels: {Labels}", imageId, labels.ToArray().ToString());
            }
            else
            {
                imageEntity.Status = ImageStatus.RecognitionFailed.ToString();
                _logger.WarnFormat("Image with ID {Id} was not updated", imageId);
            }
        }

        private static Notification BuildNotification(ImageEntity imageEntity)
        {
            var message = new Notification
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Content = imageEntity
            };

            return message;
        }
    }
}
