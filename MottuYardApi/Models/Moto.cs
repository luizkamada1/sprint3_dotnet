namespace MottuYardApi.Models
{
    public class Moto
    {
        public int Id { get; set; }
        public string Placa { get; set; } = default!;
        public string Modelo { get; set; } = default!;
        public string Status { get; set; } = "Ativa"; // Ativa, Manutenção, Inativa
        public int? ZonaId { get; set; }
        public Zona? Zona { get; set; }
    }
}