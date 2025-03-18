using Azure;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ImageService> _logger;
        private readonly INotificationProducer _notificationProducer;

        public ImageService(IStorageService storageService, IImageRepository imageRepository, INotificationProducer notificationProducer, ILogger<ImageService> logger)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _notificationProducer = notificationProducer ?? throw new ArgumentNullException(nameof(notificationProducer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

                _logger.LogInformation("Image successfully created with ID: {Id}", metadata.FileName);

                //Trigger notification
                var notification = BuildNotification(imageEntity);

                await _notificationProducer.PublishMessageAsync(notification, ((NotificationProducer)_notificationProducer).Options.QueueName);

                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating image");
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
                    _logger.LogWarning("Image with ID {Id} not found", id);
                    return;
                }

                // Delete from Blob Storage
                await _storageService.DeleteAsync(image.ObjectPath);

                // Delete metadata from CosmosDB
                await _imageRepository.DeleteAsync(id);

                _logger.LogInformation("Successfully deleted image with ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image with ID {Id}", id);
                throw;
            }
        }

        public async Task<Stream> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            try
            {
                // Retrieve image metadata
                var image = await _imageRepository.ReadAsync(id);
                if (image == null)
                {
                    _logger.LogWarning("Image with ID {Id} not found", id);
                    return null;
                }

                // Get the image file from Blob Storage
                return await _storageService.ReadAsync(image.ObjectPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image with ID {Id}", id);
                throw;
            }
        }

        public async Task DeleteAllAsync()
        {
            try
            {
                _logger.LogWarning("Starting deletion of all images from storage and database.");

                // Delete all images from CosmosDB
                await _imageRepository.DeleteAllAsync();
                _logger.LogInformation("Deleted all images from database.");

                // Delete all files from Blob Storage
                await _storageService.DeleteAllAsync();
                _logger.LogInformation("Deleted all files from blob storage.");

                _logger.LogInformation("Successfully deleted all images.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting all images.");
                throw;
            }
        }

        public async Task<IEnumerable<ImageEntity>> SearchAsync(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));

            try
            {
                return await _imageRepository.GetImagesByLabelAsync(label);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for images with label {Label}", label);
                throw;
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
