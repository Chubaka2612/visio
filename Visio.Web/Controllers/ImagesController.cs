using Microsoft.AspNetCore.Mvc;
using Visio.Data.Core;
using Visio.Services.ImageService;

namespace Visio.Web.Controllers
{
    public class ImagesController : Controller
    {
        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _imageService.GetAllImagesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == string.Empty)
            {
                return BadRequest("Invalid image ID.");
            }

            try
            {
                await _imageService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return StatusCode(500, $"An error occurred while deleting images by ID: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                await _imageService.DeleteAllAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting all images.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            var image = await _imageService.GetAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                ModelState.AddModelError(nameof(label), "Label cannot be empty.");
                return View();
            }

            try
            {
                var images = await _imageService.SearchAsync(label);
                if (images == null || !images.Any())
                {
                    ViewData["Message"] = "No images found for the specified label.";
                    return View();
                }

                return View("SearchResults", images);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error searching for images: {ex.Message}";
                return View();
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select a file.");
            }

            try
            {
                using var fileStream = file.OpenReadStream();

                var metadata = new FileMetadata
                {
                    FileName = file.FileName,
                    Size = file.Length,
                };

                await _imageService.CreateAsync(fileStream, metadata);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding image for recognition.");
            }
        }
    }
}
