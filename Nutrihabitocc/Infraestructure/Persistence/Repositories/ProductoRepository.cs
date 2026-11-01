using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Persistence.Repositories;

public class ProductoRepository: IProductoRepository
{
    private readonly AppDbContext _context;

    public ProductoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> CountAsync(ProductoFiltro filtro)
    {
        return BuildQuery(filtro).CountAsync();
    }

    public async Task<ICollection<Producto>> GetPagedAsync(ProductoFiltro filtro)
    {
        var page = filtro.Pagina <= 0 ? 1 : filtro.Pagina;
        var pageSize = filtro.PorPagina <= 0 ? 50 : Math.Min(filtro.PorPagina, 200);

        return await BuildQuery(filtro)
            .OrderBy(producto => producto.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Producto?> GetByIdAsync(long id)
    {
        return await _context.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(producto => producto.Id == id);
    }

    public async Task<ICollection<PrecioHistorial>> GetPreciosAsync(long productoId)
    {
        return await _context.PreciosHistorial
            .AsNoTracking()
            .Where(precio => precio.ProductoId == productoId)
            .OrderBy(precio => precio.Fecha)
            .ToListAsync();
    }

    public async Task<ICollection<CategoriaProductoResult>> GetCategoriasAsync(string? tienda)
    {
        var query = _context.Productos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(tienda))
        {
            query = query.Where(producto => producto.Tienda == tienda);
        }

        return await query
            .GroupBy(producto => new
            {
                producto.Tienda,
                producto.Categoria,
                producto.Subcategoria
            })
            .Select(group => new CategoriaProductoResult
            {
                Tienda = group.Key.Tienda,
                Categoria = group.Key.Categoria,
                Subcategoria = group.Key.Subcategoria,
                Productos = group.Count()
            })
            .OrderBy(result => result.Tienda)
            .ThenBy(result => result.Categoria)
            .ThenBy(result => result.Subcategoria)
            .ToListAsync();
    }

    public async Task<ICollection<CorridaScraping>> GetUltimasCorridasAsync(int limit)
    {
        return await _context.CorridasScraping
            .AsNoTracking()
            .OrderByDescending(corrida => corrida.Id)
            .Take(limit)
            .ToListAsync();
    }

    public Task<int> CountTotalAsync()
    {
        return _context.Productos.AsNoTracking().CountAsync();
    }

    public async Task<ICollection<string>> GetTiendasAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Select(producto => producto.Tienda)
            .Distinct()
            .OrderBy(tienda => tienda)
            .ToListAsync();
    }

    private IQueryable<Producto> BuildQuery(ProductoFiltro filtro)
    {
        var query = _context.Productos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Tienda))
        {
            query = query.Where(producto => producto.Tienda == filtro.Tienda);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Categoria))
        {
            query = query.Where(producto => producto.Categoria == filtro.Categoria);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Subcategoria))
        {
            query = query.Where(producto => producto.Subcategoria == filtro.Subcategoria);
        }

        if (filtro.SoloDisponibles)
        {
            query = query.Where(producto => producto.Disponible);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            var term = $"%{filtro.Q.Trim()}%";
            query = query.Where(producto =>
                EF.Functions.ILike(producto.Nombre, term) ||
                (producto.Marca != null && EF.Functions.ILike(producto.Marca, term)));
        }

        return query;
    }
}
