namespace RoomApi.Models.Dto
{
    public class ImgRoomDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
    }
}
