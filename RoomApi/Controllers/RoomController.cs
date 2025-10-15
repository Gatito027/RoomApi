using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using RoomApi.Data;
using RoomApi.Models.Dto;

namespace RoomApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;
        private readonly ResponseDto _response;

        private readonly string[] _allowedTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public RoomController(AppDbContext db, IMapper mapper, Cloudinary cloudinary)
        {
            _db = db;
            _mapper = mapper;
            _cloudinary = cloudinary;
            _response = new ResponseDto();
        }
        [HttpGet]
        public async Task<ResponseDto> Get()
        {
            try
            {
                var query = _db.Collection("roomsForms");
                var snapshot = await query.GetSnapshotAsync();

                var rooms = new List<Dictionary<string, object>>();
                foreach (var document in snapshot.Documents)
                {
                    var data = document.ToDictionary();
                    data["Id"] = document.Id;

                    // Convertir timestamps si existen
                    if (data.TryGetValue("CreatedAt", out var createdAtObj) && createdAtObj is Timestamp createdAt)
                        data["CreatedAt"] = createdAt.ToDateTime().ToString("o");

                    if (data.TryGetValue("UpdatedAt", out var updatedAtObj) && updatedAtObj is Timestamp updatedAt)
                        data["UpdatedAt"] = updatedAt.ToDateTime().ToString("o");

                    rooms.Add(data);
                }

                _response.Result = rooms;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet("{id}")]
        public async Task<ResponseDto> Get(string id)
        {
            try
            {
                var docRef = _db.Collection("roomsForms").Document(id);
                var snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    var data = snapshot.ToDictionary();
                    data["Id"] = snapshot.Id;

                    // Convertir timestamps si existen
                    if (data.TryGetValue("CreatedAt", out var createdAtObj) && createdAtObj is Timestamp createdAt)
                        data["CreatedAt"] = createdAt.ToDateTime().ToString("o");

                    if (data.TryGetValue("UpdatedAt", out var updatedAtObj) && updatedAtObj is Timestamp updatedAt)
                        data["UpdatedAt"] = updatedAt.ToDateTime().ToString("o");

                    _response.Result = data;
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Message = "Room not found";
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost]
        public async Task<ResponseDto> Post([FromForm] RoomDto room)
        {
            try
            {
                var docRef = _db.Collection("roomsForms").Document();
                var imagenes = new List<string>();

                if (room.NuevasImagenes != null && room.NuevasImagenes.Any())
                {
                    foreach (var file in room.NuevasImagenes)
                    {
                        if (!_allowedTypes.Contains(file.ContentType))
                            continue;

                        if (file.Length > MaxFileSize)
                            continue;

                        await using var stream = file.OpenReadStream();
                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(file.FileName, stream),
                            Folder = "rooms",
                            Transformation = new Transformation().Crop("limit").Width(1024).Height(1024)
                        };

                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                            imagenes.Add(uploadResult.SecureUrl.ToString());
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
                _response.Result = new { Id = docRef.Id };
                _response.Message = "Room created successfully";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Error creating room: {ex.Message}";
            }
            return _response;
        }

        [HttpPut("{id}")]
        public async Task<ResponseDto> Put(string id, [FromForm] RoomDto room)
        {
            try
            {
                var docRef = _db.Collection("roomsForms").Document(id);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Room not found";
                    return _response;
                }

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

                // Eliminar imágenes
                if (room.ImagenesAEliminar != null && room.ImagenesAEliminar.Any())
                {
                    foreach (var url in room.ImagenesAEliminar)
                    {
                        var publicId = GetPublicIdFromUrl(url);
                        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                        imagenesActuales.Remove(url);
                    }
                }

                // Subir nuevas imágenes
                if (room.NuevasImagenes != null && room.NuevasImagenes.Any())
                {
                    foreach (var file in room.NuevasImagenes)
                    {
                        if (!_allowedTypes.Contains(file.ContentType))
                            continue;

                        if (file.Length > MaxFileSize)
                            continue;

                        await using var stream = file.OpenReadStream();
                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(file.FileName, stream),
                            Folder = "rooms",
                            Transformation = new Transformation().Crop("limit").Width(1024).Height(1024)
                        };

                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                            imagenesActuales.Add(uploadResult.SecureUrl.ToString());
                    }
                }

                updateData["Imagenes"] = imagenesActuales;

                await docRef.UpdateAsync(updateData);
                _response.Result = new { Id = id };
                _response.Message = "Room updated successfully";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Error updating room: {ex.Message}";
            }
            return _response;
        }

        [HttpDelete("{id}")]
        public async Task<ResponseDto> Delete(string id)
        {
            try
            {
                var docRef = _db.Collection("roomsForms").Document(id);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Room not found";
                    return _response;
                }

                await docRef.DeleteAsync();
                _response.Message = "Room deleted successfully";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Error deleting room: {ex.Message}";
            }
            return _response;
        }

        [HttpDelete("user/{userId}")]
        public async Task<ResponseDto> DeleteByUserId(string userId)
        {
            try
            {
                var query = _db.Collection("roomsForms").WhereEqualTo("UserId", userId);
                var snapshot = await query.GetSnapshotAsync();

                if (snapshot.Count == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "No rooms found for this user";
                    return _response;
                }

                foreach (var doc in snapshot.Documents)
                {
                    var data = doc.ToDictionary();

                    // 🧹 Eliminar imágenes de Cloudinary si existen
                    if (data.TryGetValue("Imagenes", out var imagenesObj) && imagenesObj is List<object> imagenes)
                    {
                        foreach (var img in imagenes)
                        {
                            var url = img.ToString();
                            var publicId = GetPublicIdFromUrl(url);
                            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                        }
                    }

                    // 🗑️ Eliminar documento
                    await doc.Reference.DeleteAsync();
                }

                _response.Message = $"Deleted {snapshot.Count} rooms for user {userId}";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Error deleting rooms: {ex.Message}";
            }

            return _response;
        }

        private string GetPublicIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var segments = uri.Segments;
            var fileName = segments.Last(); // ej. "imagen.jpg"
            var folder = segments[segments.Length - 2].TrimEnd('/'); // ej. "rooms"
            var publicId = $"{folder}/{Path.GetFileNameWithoutExtension(fileName)}";
            return publicId;
        }
    }
}