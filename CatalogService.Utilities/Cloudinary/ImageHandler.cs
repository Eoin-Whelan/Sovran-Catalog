using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Utilities.Cloudinary
{
    public class ImageHandler
    {
        private readonly Account _account;
        public ImageHandler()
        {

            _account = new Account
            {
                Cloud = "sovran-merch",
                ApiKey = "228678378674647",
                ApiSecret = "khwW-o2cq6SmVssVY1K5B7t5U8s"
            };
        }

        public Uri PostImage(string image, string username, string location)
        {
            CloudinaryDotNet.Cloudinary cloudinary = new CloudinaryDotNet.Cloudinary(_account);
            cloudinary.Api.Secure = true;

            var uploadParams = new ImageUploadParams()
            {
                PublicId = $"merchants/{username}/{location}",
                EagerTransforms = new List<Transformation>()
                {
                    new EagerTransformation().Width(400).Height(400)
                },
                
                File = new FileDescription(@"data:image/jpg;base64," + image)
            };
            var uploadResult = cloudinary.Upload(uploadParams);
            var result = uploadResult.Url;
            return result;
        }
    }
}
