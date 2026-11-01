namespace Nutrihabitocc.Api.Dtos.Producto;

public class ProductoResponseDto
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
    public string? ImagenUrlOrigen { get; set; }
    public string? Imagen { get; set; }
    public string? Descripcion { get; set; }
    public bool Disponible { get; set; }
    public bool Revisar { get; set; }
    public DateTime? PrimeraVezVisto { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
}

public class ProductoPaginadoResponseDto
{
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int PorPagina { get; set; }
    public ICollection<ProductoResponseDto> Productos { get; set; } = new List<ProductoResponseDto>();
}

public class PrecioHistorialResponseDto
{
    public DateTime? Fecha { get; set; }
    public decimal? Precio { get; set; }
    public decimal? PrecioRegular { get; set; }
}

public class CategoriaProductoResponseDto
{
    public string Tienda { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string? Subcategoria { get; set; }
    public int Productos { get; set; }
}

public class ScrapingStatusResponseDto
{
    public int TotalProductos { get; set; }
    public ICollection<string> Tiendas { get; set; } = new List<string>();
    public ICollection<CorridaScrapingResponseDto> UltimasCorridas { get; set; } = new List<CorridaScrapingResponseDto>();
}

public class CorridaScrapingResponseDto
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
