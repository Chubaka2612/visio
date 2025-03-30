using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Visio.Recognition.Services
{
    public class ImageRecognitionService : IImageRecognitionService
    {
        private readonly ComputerVisionClient _computerVisionClient;

        public ImageRecognitionService(ComputerVisionClient computerVisionClient)
        {
            _computerVisionClient = computerVisionClient;
        }

        public async Task<List<string>> RecognizeImageAsync(string imageUrl)
        {
         
            var analysis = await _computerVisionClient.AnalyzeImageAsync(imageUrl, [VisualFeatureTypes.Tags]);

            if (analysis.Tags.Count == 0)
            {
                return new List<string>() { "no tags" };
            }
            return analysis.Tags.Select(tag => tag.Name).ToList();
        }
    }
}
