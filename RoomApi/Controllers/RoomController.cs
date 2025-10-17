using Microsoft.AspNetCore.Mvc;
using RoomApi.Models.Dto;
using RoomApi.Services;

namespace RoomApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly RoomGetService _roomGetService;
        private readonly RoomGetByIdService _roomGetByIdService;
        private readonly RoomCreateService _roomCreateService;
        private readonly RoomEditService _roomEditService;
        private readonly RoomDeleteService _roomDeleteService;
        private readonly RoomDeleteByUserService _roomDeleteByUserService;

        public RoomController(
            RoomGetService roomGetService,
            RoomGetByIdService roomGetByIdService,
            RoomCreateService roomCreateService,
            RoomEditService roomEditService,
            RoomDeleteService roomDeleteService,
            RoomDeleteByUserService roomDeleteByUserService
        )
        {
            _roomGetService = roomGetService;
            _roomGetByIdService = roomGetByIdService;
            _roomCreateService = roomCreateService;
            _roomEditService = roomEditService;
            _roomDeleteService = roomDeleteService;
            _roomDeleteByUserService = roomDeleteByUserService;
        }

        [HttpGet]
        public async Task<ResponseDto> Get()
        {
            var response = new ResponseDto();
            try
            {
                var rooms = await _roomGetService.ExecuteAsync();
                response.Result = rooms;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpGet("{id}")]
        public async Task<ResponseDto> Get(string id)
        {
            var response = new ResponseDto();
            try
            {
                var room = await _roomGetByIdService.ExecuteAsync(id);
                if (room != null)
                {
                    response.Result = room;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Room not found";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        public async Task<ResponseDto> Post([FromForm] RoomDto room)
        {
            var response = new ResponseDto();
            try
            {
                var result = await _roomCreateService.ExecuteAsync(room);
                response.Result = result;
                response.Message = "Room created successfully";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error creating room: {ex.Message}";
            }
            return response;
        }

        [HttpPut("{id}")]
        public async Task<ResponseDto> Put(string id, [FromForm] RoomDto room)
        {
            var response = new ResponseDto();
            try
            {
                var result = await _roomEditService.ExecuteAsync(id, room);
                response.Result = result;
                response.Message = "Room updated successfully";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error updating room: {ex.Message}";
            }
            return response;
        }

        [HttpDelete("{id}")]
        public async Task<ResponseDto> Delete(string id)
        {
            var response = new ResponseDto();
            try
            {
                await _roomDeleteService.ExecuteAsync(id);
                response.Message = "Room and associated images deleted successfully";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error deleting room: {ex.Message}";
            }
            return response;
        }

        [HttpDelete("user/{userId}")]
        public async Task<ResponseDto> DeleteByUserId(string userId)
        {
            var response = new ResponseDto();
            try
            {
                var count = await _roomDeleteByUserService.ExecuteAsync(userId);
                response.Message = $"Deleted {count} rooms for user {userId}";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error deleting rooms: {ex.Message}";
            }
            return response;
        }
    }
}