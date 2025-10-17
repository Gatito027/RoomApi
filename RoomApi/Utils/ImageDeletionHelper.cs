using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace RoomApi.Utils
{
    public static class ImageDeletionHelper
    {
        public static async Task DeleteImagesAsync(IEnumerable<string> imageUrls, Cloudinary cloudinary)
        {
            foreach (var url in imageUrls)
            {
                var publicId = CloudinaryHelper.GetPublicIdFromUrl(url);
                await cloudinary.DestroyAsync(new DeletionParams(publicId));
            }
        }
    }
}