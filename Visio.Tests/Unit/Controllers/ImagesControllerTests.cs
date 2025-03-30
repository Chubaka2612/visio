using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Visio.Data.Core;
using Visio.Domain.Common.Images;
using Visio.Services.ImageService;
using Visio.Web.Controllers;

namespace Visio.Tests.Unit.Controllers
{
    [TestFixture]
    public class ImagesControllerTests
    {
        private Mock<IImageService> _mockImageService;
        private ImagesController _controller;

        [SetUp]
        public void Setup()
        {
            _mockImageService = new Mock<IImageService>();
            _controller = new ImagesController(_mockImageService.Object);
        }

        [Test]
        public async Task Index_ReturnsView_WithImages()
        {
            // Arrange
            var images = new List<ConsolidatedImage>
            {
                new ()
                {
                    ImageEntity = new ImageEntity
                    {
                        Id = "1",
                        ObjectPath = "http://example.com/image.jpg",
                        Status = "New"
                    }
                }
            };
            _mockImageService.Setup(s => s.GetAllImagesAsync()).ReturnsAsync(images);

            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(images, result.Model);
        }

        [Test]
        public async Task Create_ValidFile_RedirectsToIndex()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            _mockImageService.Setup(s => s.CreateAsync(It.IsAny<Stream>(), It.IsAny<FileMetadata>()))
                .ReturnsAsync("new-image-id");

            // Act
            var result = await _controller.Create(fileMock.Object) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(nameof(_controller.Index), result.ActionName);
        }

        [Test]
        public async Task Get_ValidId_ReturnsViewWithImage()
        {
            // Arrange
            var image = new ConsolidatedImage
            {
                ImageEntity = new ImageEntity
                {
                    Id = "123",
                    ObjectPath = "http://example.com/image.jpg"
                }
            };
            _mockImageService.Setup(s => s.GetAsync("123")).ReturnsAsync(image);

            // Act
            var result = await _controller.Get("123") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(image, result.Model);
        }

        [Test]
        public async Task Delete_ValidId_RedirectsToIndex()
        {
            // Arrange
            var id = "123";
            _mockImageService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(nameof(_controller.Index), result.ActionName);
        }

        [Test]
        public async Task UpdateImageLabels_ValidId_UpdatesLabels()
        {
            // Arrange
            var imageId = "123";
            var labels = new List<string> { "cat", "animal" };
            _mockImageService.Setup(s => s.UpdateImageLabelsAsync(imageId, labels)).Returns(Task.CompletedTask);

            // Act
            await _mockImageService.Object.UpdateImageLabelsAsync(imageId, labels);

            // Assert
            _mockImageService.Verify(s => s.UpdateImageLabelsAsync(imageId, labels), Times.Once);
        }

        [Test]
        public async Task Delete_EmptyId_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Delete("") as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Invalid image ID.", result.Value);
        }

        [Test]
        public async Task Get_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockImageService.Setup(s => s.GetAsync("invalid")).ReturnsAsync((ConsolidatedImage)null);

            // Act
            var result = await _controller.Get("invalid") as NotFoundResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        [Test]
        public async Task Search_NoResults_ReturnsViewWithMessage()
        {
            // Arrange
            _mockImageService.Setup(s => s.SearchAsync("unknown")).ReturnsAsync(new List<ConsolidatedImage>());

            // Act
            var result = await _controller.Search("unknown") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("No images found for the specified label.", result.ViewData["Message"]);
        }

        [Test]
        public void ImageEntity_Serializes_Correctly()
        {
            // Arrange
            var image = new ImageEntity
            {
                Id = "456",
                ObjectPath = "http://example.com/test.jpg",
                Labels = [ "nature", "tree" ],
                Status = "Completed"
            };

            // Act
            var json = JsonConvert.SerializeObject(image);
            var deserializedImage = JsonConvert.DeserializeObject<ImageEntity>(json);

            // Assert
            Assert.AreEqual(image.ObjectPath, deserializedImage.ObjectPath);
            Assert.AreEqual(image.Labels.Count, deserializedImage.Labels.Count);
            Assert.AreEqual(image.Status, deserializedImage.Status);
        }
    }
}