using Newtonsoft.Json;
using Visio.Domain.Core;

namespace Visio.Domain.Common.Images
{
    public class ImageEntity : Entity<string>
    {
        [JsonProperty("object_path")]
        public string ObjectPath { get; set; }

        [JsonProperty("object_size")]
        public string ObjectSize { get; set; }

        [JsonProperty("time_updated")]
        public DateTime TimeUpdated { get; set; } = DateTime.UtcNow;

        [JsonProperty("labels")]
        public List<string> Labels { get; set; } = new List<string>();

        [JsonProperty("status")]
        public string Status { get; set; }

        public ImageEntity() : base() => Id = Guid.NewGuid().ToString();
    }
}
