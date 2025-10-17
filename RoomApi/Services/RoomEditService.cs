using RoomApi.Data;
using RoomApi.Models.Dto;
using RoomApi.Utils;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Google.Cloud.Firestore;

namespace RoomApi.Services
{
    public class RoomEditService
    {
        private readonly AppDbContext _db;
        private readonly Cloudinary _cloudinary;
        private readonly ContentSafetyValidator _contentSafetyValidator;

        public RoomEditService(AppDbContext db, Cloudinary cloudinary, ContentSafetyValidator contentSafetyValidator)
        {
            _db = db;
            _cloudinary = cloudinary;
            _contentSafetyValidator = contentSafetyValidator;
        }

        public async Task<object> ExecuteAsync(string id, RoomDto room)
        {
            var docRef = _db.Collection("roomsForms").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new Exception("Room not found");

            if (await _contentSafetyValidator.TextContainsHarmfulContentAsync(room.Nombre ?? "") ||
                await _contentSafetyValidator.TextContainsHarmfulContentAsync(room.Descripcion ?? ""))
                throw new Exception("El nombre o la descripción contienen contenido inapropiado.");

            var updateData = new Dictionary<string, object>
            {
                { "Nombre", room.Nombre ?? "" },
                { "Precio", room.Precio },
                { "Descripcion", room.Descripcion ?? "" },
                { "Capacidad", room.Capacidad },
                { "Disponible", room.Disponible },
                { "Servicios", room.Servicios ?? new List<string>() },
                { "UserId", room.UserId ?? "" },
                { "Ubicacion", room.Ubicacion ?? "" },
                { "UpdatedAt", Timestamp.GetCurrentTimestamp() }
            };

            var currentData = snapshot.ToDictionary();
            var imagenesActuales = currentData.ContainsKey("Imagenes")
                ? ((List<object>)currentData["Imagenes"]).Select(i => i.ToString()).ToList()
                : new List<string>();

            // 🧹 Eliminar imágenes solicitadas
            if (room.ImagenesAEliminar != null && room.ImagenesAEliminar.Any())
            {
                await ImageDeletionHelper.DeleteImagesAsync(room.ImagenesAEliminar, _cloudinary);
                imagenesActuales = imagenesActuales.Except(room.ImagenesAEliminar).ToList();
            }

            // 📤 Subir nuevas imágenes y moderar por URL
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

                    imagenesActuales.Add(imageUrl);
                }
            }

            updateData["Imagenes"] = imagenesActuales;
            await docRef.UpdateAsync(updateData);

            return new { Id = id };
        }
    }
}