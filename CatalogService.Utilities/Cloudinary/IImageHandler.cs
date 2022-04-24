
namespace CatalogService.Utilities.Cloudinary
{
    /// <summary>
    /// Contract for ImageHandler class. Used for dependency injection.
    /// </summary>
    public interface IImageHandler
    {
        string PostProductImg(string image, string username, string itemName);
    }
}