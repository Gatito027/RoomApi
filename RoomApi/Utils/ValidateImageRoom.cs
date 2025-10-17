namespace RoomApi.Utils
{
    public static class ValidateImageRoom
    {
        private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public static bool IsValid(IFormFile file)
        {
            return AllowedTypes.Contains(file.ContentType) && file.Length <= MaxFileSize;
        }
    }
}