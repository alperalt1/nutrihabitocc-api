namespace Nutrihabitocc.Domain.Entities;

public class CorridaScraping
{
    public long Id { get; set; }
    public string Tienda { get; set; } = string.Empty;
    public DateTime? Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public string? Estado { get; set; }
    public int? Vistos { get; set; }
    public int? Nuevos { get; set; }
    public int? Actualizados { get; set; }
    public int? NoDisponibles { get; set; }
    public int? Errores { get; set; }
    public string? Detalle { get; set; }
}
