using AutoMapper;
using RoomApi.Models;
using RoomApi.Models.Dto;

namespace RoomApi
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Room, RoomDto>().ReverseMap();
        }
    }
}
