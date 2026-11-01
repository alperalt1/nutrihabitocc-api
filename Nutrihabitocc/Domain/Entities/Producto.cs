namespace Nutrihabitocc.Domain.Entities;

public class Producto
{
    public long Id { get; set; }
    public string Tienda { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Categoria { get; set; }
    public string? Subcategoria { get; set; }
    public string? Presentacion { get; set; }
    public decimal? Precio { get; set; }
    public decimal? PrecioRegular { get; set; }
    public string? PrecioUnidad { get; set; }
    public string? Url { get; set; }
    public string? ImagenUrl { get; set; }
    public string? ImagenKey { get; set; }
    public string? Descripcion { get; set; }
    public bool Disponible { get; set; } = true;
    public bool Revisar { get; set; }
    public DateTime? PrimeraVezVisto { get; set; }
    public DateTime? UltimaVezVisto { get; set; }
    public DateTime? UltimaActualizacion { get; set; }

    public ICollection<PrecioHistorial> Historial { get; set; } = new List<PrecioHistorial>();
}
