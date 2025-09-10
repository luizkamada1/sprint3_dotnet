namespace MottuYardApi.Models
{
    public class Patio
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public string Cidade { get; set; } = default!;
        public string Estado { get; set; } = default!;
        public List<Zona> Zonas { get; set; } = new();
    }
}