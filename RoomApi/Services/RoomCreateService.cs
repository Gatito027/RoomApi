using RoomApi.Data;
using RoomApi.Models.Dto;
using RoomApi.Utils;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Google.Cloud.Firestore;

namespace RoomApi.Services
{
    public class RoomCreateService
    {
        private readonly AppDbContext _db;
        private readonly Cloudinary _cloudinary;
        private readonly ContentSafetyValidator _contentSafetyValidator;

        public RoomCreateService(AppDbContext db, Cloudinary cloudinary, ContentSafetyValidator contentSafetyValidator)
        {
            _db = db;
            _cloudinary = cloudinary;
            _contentSafetyValidator = contentSafetyValidator;
        }

        public async Task<object> ExecuteAsync(RoomDto room)
        {
            if (await _contentSafetyValidator.TextContainsHarmfulContentAsync(room.Nombre ?? "") ||
                await _contentSafetyValidator.TextContainsHarmfulContentAsync(room.Descripcion ?? ""))
                throw new Exception("El nombre o la descripción contienen contenido inapropiado.");

            var docRef = _db.Collection("roomsForms").Document();
            var imagenes = new List<string>();

            if (room.NuevasImagenes != null && room.NuevasImagenes.Any())
            {
                foreach (var file in room.NuevasImagenes)
                {
                    if (!ValidateImageRoom.IsValid(file))
                        continue;

                    await using var stream = file.OpenReadStream();
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = "rooms",
                        Transformation = new Transformation().Crop("limit").Width(1024).Height(1024)
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    var imageUrl = uploadResult.SecureUrl?.ToString();
                    if (string.IsNullOrEmpty(imageUrl)) continue;

                    if (await _contentSafetyValidator.ImageContainsHarmfulContentAsync(new Uri(imageUrl)))
                    {
                        var publicId = CloudinaryHelper.GetPublicIdFromUrl(imageUrl);
                        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                        throw new Exception("Una de las imágenes subidas contiene contenido inapropiado.");
                    }

                    imagenes.Add(imageUrl);
                }
            }

            var roomData = new Dictionary<string, object>
            {
                { "Nombre", room.Nombre ?? "" },
                { "Precio", room.Precio },
                { "Descripcion", room.Descripcion ?? "" },
                { "Capacidad", room.Capacidad },
                { "Disponible", room.Disponible },
                { "Servicios", room.Servicios ?? new List<string>() },
                { "UserId", room.UserId ?? "" },
                { "Ubicacion", room.Ubicacion ?? "" },
                { "Imagenes", imagenes },
                { "CreatedAt", Timestamp.GetCurrentTimestamp() },
                { "UpdatedAt", Timestamp.GetCurrentTimestamp() }
            };

            await docRef.SetAsync(roomData);
            return new { Id = docRef.Id };
        }
    }
}