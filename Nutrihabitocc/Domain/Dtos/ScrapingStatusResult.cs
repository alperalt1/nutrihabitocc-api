namespace Nutrihabitocc.Domain.Dtos;

public class ScrapingStatusResult
{
    public int TotalProductos { get; set; }
    public ICollection<string> Tiendas { get; set; } = new List<string>();
    public ICollection<CorridaScrapingResult> UltimasCorridas { get; set; } = new List<CorridaScrapingResult>();
}

public class CorridaScrapingResult
{
    public string Tienda { get; set; } = string.Empty;
    public string? Estado { get; set; }
    public DateTime? Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public int? Vistos { get; set; }
    public int? Nuevos { get; set; }
    public int? Actualizados { get; set; }
    public int? NoDisponibles { get; set; }
    public int? Errores { get; set; }
}
