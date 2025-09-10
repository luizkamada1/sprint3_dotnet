namespace MottuYardApi.Models
{
    public class Zona
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public int PatioId { get; set; }
        public Patio? Patio { get; set; }
        public List<Moto> Motos { get; set; } = new();
    }
}