using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IProductoRepository
{
    Task<int> CountAsync(ProductoFiltro filtro);
    Task<ICollection<Producto>> GetPagedAsync(ProductoFiltro filtro);
    Task<Producto?> GetByIdAsync(long id);
    Task<ICollection<PrecioHistorial>> GetPreciosAsync(long productoId);
    Task<ICollection<CategoriaProductoResult>> GetCategoriasAsync(string? tienda);
    Task<ICollection<CorridaScraping>> GetUltimasCorridasAsync(int limit);
    Task<int> CountTotalAsync();
    Task<ICollection<string>> GetTiendasAsync();
}
