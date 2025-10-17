using RoomApi.Data;
using RoomApi.Utils;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Google.Cloud.Firestore;

namespace RoomApi.Services
{
    public class RoomDeleteService
    {
        private readonly AppDbContext _db;
        private readonly Cloudinary _cloudinary;

        public RoomDeleteService(AppDbContext db, Cloudinary cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }

        public async Task ExecuteAsync(string id)
        {
            var docRef = _db.Collection("roomsForms").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new Exception("Room not found");

            var data = snapshot.ToDictionary();
            if (data.TryGetValue("Imagenes", out var imagenesObj) && imagenesObj is List<object> imagenes)
            {
                await ImageDeletionHelper.DeleteImagesAsync(imagenes.Select(i => i.ToString()), _cloudinary);
            }

            await docRef.DeleteAsync();
        }
    }
}