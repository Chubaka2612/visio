using System.Text.Json.Serialization;
namespace Visio.Domain
{
    public class ImageData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string ObjectPath { get; set; }

        public string ObjectSize { get; set; }

        public DateTime TimeAdded { get; set; } = DateTime.UtcNow;

        public DateTime TimeUpdated { get; set; } = DateTime.UtcNow;

        public List<string> Labels { get; set; } = new List<string>();

        public ImageStatus Status { get; set; }
    }
}
