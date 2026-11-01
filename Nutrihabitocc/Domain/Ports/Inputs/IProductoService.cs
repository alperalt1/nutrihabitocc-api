using Nutrihabitocc.Domain.Dtos;

namespace Nutrihabitocc.Domain.Ports.Inputs;

public interface IProductoService
{
    Task<ProductoPaginadoResult> GetProductosAsync(ProductoFiltro filtro);
    Task<ProductoResult?> GetProductoByIdAsync(long id);
    Task<S3ObjectResult?> GetImagenAsync(long productoId, CancellationToken cancellationToken = default);
    Task<ICollection<PrecioHistorialResult>> GetPreciosAsync(long productoId);
    Task<ICollection<CategoriaProductoResult>> GetCategoriasAsync(string? tienda);
    Task<ScrapingStatusResult> GetScrapingStatusAsync();
}
