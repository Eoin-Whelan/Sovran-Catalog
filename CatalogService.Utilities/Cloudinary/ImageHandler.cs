using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Utilities.Cloudinary
{
    /// <summary>
    /// ImageHandler is a service class for communication with Sovran's Cloudinary account.<br></br>
    /// 
    /// It generates links from the encoded image files to allow for dynamic profile and product images.<br></br><br></br>
    /// This is strictly used in registration and update account flows.
    /// </summary>
    public class ImageHandler : IImageHandler
    {
        private readonly Account _account;
        public ImageHandler(Account account)
        {
            _account = account;
        }

        /// <summary>
        /// PostProductImg takes the encoded image, username and the itemName itself in <br></br> order to generate
        /// a dynamic product page image for storefront generation.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="username"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public string PostProductImg(string image, string username, string itemName)
        {
            var uploadParams = new ImageUploadParams()
            {
                PublicId = $"merchants/{username}/catalog/{itemName}",
                Transformation = new Transformation().Width(400).Height(400).Crop("limit"),
                File = new FileDescription(image)
            };

            return Post(uploadParams);
        }

        /// <summary>
        /// Post acts as the common thread for both product and profile image uploading. Takes parameters and <br></br>
        /// posts them to Cloudinary account.
        /// </summary>
        /// <param name="imgParams"></param>
        /// <returns>Absolute URL of image desired.</returns>
        private string Post(ImageUploadParams imgParams)
        {
            CloudinaryDotNet.Cloudinary client = new CloudinaryDotNet.Cloudinary(_account);
            client.Api.Secure = true;

            var uploadResult = client.Upload(imgParams);
            var result = uploadResult.Url;
            return result.AbsoluteUri;
        }

    }
}
