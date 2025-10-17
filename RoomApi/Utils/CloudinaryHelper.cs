namespace RoomApi.Utils
{
    public static class CloudinaryHelper
    {
        public static string GetPublicIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var segments = uri.Segments;
            var fileName = segments.Last();
            var folder = segments[segments.Length - 2].TrimEnd('/');
            return $"{folder}/{Path.GetFileNameWithoutExtension(fileName)}";
        }
    }
}