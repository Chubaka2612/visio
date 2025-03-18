
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Visio.Recognition.Services
{
    public interface IImageRecognitionService
    {
        Task<List<string>> RecognizeImageAsync(string imageUrl);
    }
}
