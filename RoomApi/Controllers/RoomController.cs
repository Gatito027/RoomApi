using AutoMapper;
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
        private ResponseDto _response;
        private IMapper _mapper;

        public RoomController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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
        public async Task<ResponseDto> Post([FromBody] RoomDto room)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Invalid model data";
                    return _response;
                }

                var docRef = _db.Collection("roomsForms").Document();
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
        public async Task<ResponseDto> Put(string id, [FromBody] RoomDto room)
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
    }
}