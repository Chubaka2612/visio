
namespace Visio.Domain
{
    public enum ImageStatus
    {
        Pending,    // Image is uploaded but not yet processed
        Processing, // Image is being processed (e.g., recognition, resizing)
        Completed,  // Image processing is completed
        Failed,     // Processing failed due to an error
        Archived    // Image is archived and no longer actively used
    }
}
