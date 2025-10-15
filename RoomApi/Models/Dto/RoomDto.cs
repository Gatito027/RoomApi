using System.ComponentModel.DataAnnotations;

namespace RoomApi.Models.Dto
{
    public class RoomDto
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public double Precio { get; set; }

        public string Descripcion { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Capacidad { get; set; }

        public bool Disponible { get; set; } = true;

        public List<string> Servicios { get; set; } = new();

        [Required]
        public string UserId { get; set; }

        public string Ubicacion { get; set; }
    }
}
