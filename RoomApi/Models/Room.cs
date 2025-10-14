using System.Collections.Specialized;

namespace RoomApi.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Capacidad { get; set; }
        public bool Disponible { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Servicios { get; set; }
        public double Precio { get; set; }
        public string UserId { get; set; }
        public string Ubicacion { get; set; }
    }
}
