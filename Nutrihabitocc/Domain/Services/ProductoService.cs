using Microsoft.AspNetCore.Http;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Domain.Services;

public class ProductoService: IProductoService
{
    private readonly IProductoRepository _productoRepository;
    private readonly IS3StorageService _s3StorageService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ProductoService> _logger;

    public ProductoService(
        IProductoRepository productoRepository,
        IS3StorageService s3StorageService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ProductoService> logger)
    {
        _productoRepository = productoRepository;
        _s3StorageService = s3StorageService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<ProductoPaginadoResult> GetProductosAsync(ProductoFiltro filtro)
    {
        var pagina = filtro.Pagina <= 0 ? 1 : filtro.Pagina;
        var porPagina = filtro.PorPagina <= 0 ? 50 : Math.Min(filtro.PorPagina, 200);
        filtro.Pagina = pagina;
        filtro.PorPagina = porPagina;

        var total = await _productoRepository.CountAsync(filtro);
        var productos = await _productoRepository.GetPagedAsync(filtro);

        return new ProductoPaginadoResult
        {
            Total = total,
            Pagina = pagina,
            PorPagina = porPagina,
            Productos = productos.Select(Map).ToList()
        };
    }

    public async Task<ProductoResult?> GetProductoByIdAsync(long id)
    {
        var producto = await _productoRepository.GetByIdAsync(id);
        return producto is null ? null : Map(producto);
    }

    public async Task<S3ObjectResult?> GetImagenAsync(
        long productoId,
        CancellationToken cancellationToken = default)
    {
        var producto = await _productoRepository.GetByIdAsync(productoId);
        if (producto is null || string.IsNullOrWhiteSpace(producto.ImagenKey))
        {
            return null;
        }

        return await _s3StorageService.GetObjectAsync(producto.ImagenKey, cancellationToken);
    }

    public async Task<ICollection<PrecioHistorialResult>> GetPreciosAsync(long productoId)
    {
        var producto = await _productoRepository.GetByIdAsync(productoId);
        if (producto is null)
        {
            throw new KeyNotFoundException("El producto no existe.");
        }

        var precios = await _productoRepository.GetPreciosAsync(productoId);

        return precios.Select(precio => new PrecioHistorialResult
        {
            Fecha = precio.Fecha,
            Precio = precio.Precio,
            PrecioRegular = precio.PrecioRegular
        }).ToList();
    }

    public Task<ICollection<CategoriaProductoResult>> GetCategoriasAsync(string? tienda)
    {
        return _productoRepository.GetCategoriasAsync(tienda);
    }

    public async Task<ScrapingStatusResult> GetScrapingStatusAsync()
    {
        var total = await _productoRepository.CountTotalAsync();
        var tiendas = await _productoRepository.GetTiendasAsync();
        var corridas = await _productoRepository.GetUltimasCorridasAsync(10);

        return new ScrapingStatusResult
        {
            TotalProductos = total,
            Tiendas = tiendas,
            UltimasCorridas = corridas.Select(corrida => new CorridaScrapingResult
            {
                Tienda = corrida.Tienda,
                Estado = corrida.Estado,
                Inicio = corrida.Inicio,
                Fin = corrida.Fin,
                Vistos = corrida.Vistos,
                Nuevos = corrida.Nuevos,
                Actualizados = corrida.Actualizados,
                NoDisponibles = corrida.NoDisponibles,
                Errores = corrida.Errores
            }).ToList()
        };
    }

    private ProductoResult Map(Producto producto)
    {
        var imageUrl = string.IsNullOrWhiteSpace(producto.ImagenKey)
            ? producto.ImagenUrl
            : BuildImageUrl(producto.Id);

        return new ProductoResult
        {
            Id = producto.Id,
            Tienda = producto.Tienda,
            Sku = producto.Sku,
            Nombre = producto.Nombre,
            Marca = producto.Marca,
            Categoria = producto.Categoria,
            Subcategoria = producto.Subcategoria,
            Presentacion = producto.Presentacion,
            Precio = producto.Precio,
            PrecioRegular = producto.PrecioRegular,
            PrecioUnidad = producto.PrecioUnidad,
            Url = producto.Url,
            ImagenUrlOrigen = producto.ImagenUrl,
            Imagen = imageUrl,
            Descripcion = producto.Descripcion,
            Disponible = producto.Disponible,
            Revisar = producto.Revisar,
            PrimeraVezVisto = producto.PrimeraVezVisto,
            UltimaActualizacion = producto.UltimaActualizacion
        };
    }

    private string BuildImageUrl(long productoId)
    {
        var imagePath = $"/api/Producto/{productoId}/imagen";
        var request = _httpContextAccessor.HttpContext?.Request;

        if (request is not null && request.Host.HasValue)
        {
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return $"{baseUrl.TrimEnd('/')}{imagePath}";
        }

        var fallback = _configuration["Api:PublicBaseUrl"];
        return string.IsNullOrWhiteSpace(fallback)
            ? imagePath
            : $"{fallback.TrimEnd('/')}{imagePath}";
    }
}
