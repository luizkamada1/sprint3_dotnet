using MottuYardApi.Models;

namespace MottuYardApi.DTOs
{
    /// <summary>Pátio logístico da Mottu.</summary>
    public record PatioDto(int Id, string Nome, string Cidade, string Estado);
    /// <summary>Criação de pátio.</summary>
    public record PatioCreateDto(string Nome, string Cidade, string Estado);
    /// <summary>Atualização de pátio.</summary>
    public record PatioUpdateDto(string Nome, string Cidade, string Estado);

    /// <summary>Zona dentro de um pátio.</summary>
    public record ZonaDto(int Id, string Nome, int PatioId, string PatioNome);
    /// <summary>Criação de zona.</summary>
    public record ZonaCreateDto(string Nome, int PatioId);
    /// <summary>Atualização de zona.</summary>
    public record ZonaUpdateDto(string Nome);

    /// <summary>Moto rastreada no pátio.</summary>
    public record MotoDto(int Id, string Placa, string Modelo, string Status, int? ZonaId, string? ZonaNome, int? PatioId, string? PatioNome)
    {
        public static MotoDto FromEntity(Moto m) =>
            new(m.Id, m.Placa, m.Modelo, m.Status,
                m.ZonaId,
                m.Zona?.Nome,
                m.Zona?.PatioId,
                m.Zona?.Patio?.Nome);
    }
    /// <summary>Criação de moto.</summary>
    public record MotoCreateDto(string Placa, string Modelo, string Status, int? ZonaId);
    /// <summary>Atualização de moto.</summary>
    public record MotoUpdateDto(string Placa, string Modelo, string Status, int? ZonaId);

    /// <summary>Movimentação de moto: alterar zona.</summary>
    public record MotoMoverDto(int NovaZonaId);

    /// <summary>Solicitação para estimar necessidade de manutenção de uma moto.</summary>
    public record MaintenancePredictionRequest(float DaysSinceMaintenance, float CompletedDeliveries, float BreakdownHistory);

    /// <summary>Resposta da predição de manutenção.</summary>
    public record MaintenancePredictionResponse(bool RequiresMaintenance, float Probability);
}