using Visio.Domain.Common.Images;

namespace Visio.Domain.Common
{
    public class Notification
    {
        public Guid Id { get; set; }

        public DateTime Timestamp { get; set; }

        public ImageEntity Content { get; set; }
    }
}
