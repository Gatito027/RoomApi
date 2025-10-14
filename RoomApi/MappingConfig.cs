// Profiles/AutoMapperProfile.cs
using AutoMapper;
using RoomApi.Models.Dto;

namespace RoomApi
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration();
            mappingConfig.CreateMap<RoomDto, Models.Room>().ReverseMap();
            return mappingConfig;
        }
    }
    // Ejemplo de mapeo si necesitas mapear entre entidades
    //CreateMap<Room, RoomDto>().ReverseMap();
}