using RoomApi.Data;
using RoomApi.Utils;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Google.Cloud.Firestore;

namespace RoomApi.Services
{
    public class RoomDeleteByUserService
    {
        private readonly AppDbContext _db;
        private readonly Cloudinary _cloudinary;

        public RoomDeleteByUserService(AppDbContext db, Cloudinary cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }

        public async Task<int> ExecuteAsync(string userId)
        {
            var query = _db.Collection("roomsForms").WhereEqualTo("UserId", userId);
            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count == 0)
                throw new Exception("No rooms found for this user");

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();
                if (data.TryGetValue("Imagenes", out var imagenesObj) && imagenesObj is List<object> imagenes)
                {
                    await ImageDeletionHelper.DeleteImagesAsync(imagenes.Select(i => i.ToString()), _cloudinary);
                }

                await doc.Reference.DeleteAsync();
            }

            return snapshot.Count;
        }
    }
}