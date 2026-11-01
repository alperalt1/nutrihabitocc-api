using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nutrihabitocc.Api.Dtos;
using Nutrihabitocc.Api.Dtos.Producto;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Ports.Inputs;

namespace Nutrihabitocc.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicies.WebStaffOnly)]
[Route("api/[controller]")]
[Route("api/Productos")]
public class ProductoController: ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly ILogger<ProductoController> _logger;

    public ProductoController(IProductoService productoService, ILogger<ProductoController> logger)
    {
        _productoService = productoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProductoPaginadoResponseDto>>> GetProductos(
        [FromQuery] string? tienda,
        [FromQuery] string? categoria,
        [FromQuery] string? subcategoria,
        [FromQuery] string? q,
        [FromQuery] bool soloDisponibles = true,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 50)
    {
        try
        {
            var result = await _productoService.GetProductosAsync(new ProductoFiltro
            {
                Tienda = tienda,
                Categoria = categoria,
                Subcategoria = subcategoria,
                Q = q,
                SoloDisponibles = soloDisponibles,
                Pagina = pagina,
                PorPagina = porPagina
            });

            return Ok(ApiResponse<ProductoPaginadoResponseDto>.Ok(Map(result)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los productos.");
            return StatusCode(500, ApiResponse<ProductoPaginadoResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [AllowAnonymous]
    [HttpGet("{id:long}/imagen")]
    public async Task<IActionResult> GetImagen(long id, CancellationToken cancellationToken)
    {
        try
        {
            var image = await _productoService.GetImagenAsync(id, cancellationToken);
            if (image is null)
            {
                return NotFound();
            }

            return File(image.Stream, image.ContentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la imagen del producto {Id}.", id);
            return StatusCode(500);
        }
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<ProductoResponseDto>>> GetProducto(long id)
    {
        try
        {
            var producto = await _productoService.GetProductoByIdAsync(id);
            if (producto is null)
            {
                return NotFound(ApiResponse<ProductoResponseDto>.Fail("El producto no existe."));
            }

            return Ok(ApiResponse<ProductoResponseDto>.Ok(Map(producto)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el producto {Id}.", id);
            return StatusCode(500, ApiResponse<ProductoResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id:long}/precios")]
    public async Task<ActionResult<ApiResponse<ICollection<PrecioHistorialResponseDto>>>> GetPrecios(long id)
    {
        try
        {
            var precios = await _productoService.GetPreciosAsync(id);
            var response = precios.Select(precio => new PrecioHistorialResponseDto
            {
                Fecha = precio.Fecha,
                Precio = precio.Precio,
                PrecioRegular = precio.PrecioRegular
            }).ToList();

            return Ok(ApiResponse<ICollection<PrecioHistorialResponseDto>>.Ok(response));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ICollection<PrecioHistorialResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los precios del producto {Id}.", id);
            return StatusCode(500, ApiResponse<ICollection<PrecioHistorialResponseDto>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpGet("categorias")]
    public async Task<ActionResult<ApiResponse<ICollection<CategoriaProductoResponseDto>>>> GetCategorias(
        [FromQuery] string? tienda)
    {
        try
        {
            var categorias = await _productoService.GetCategoriasAsync(tienda);
            var response = categorias.Select(categoria => new CategoriaProductoResponseDto
            {
                Tienda = categoria.Tienda,
                Categoria = categoria.Categoria,
                Subcategoria = categoria.Subcategoria,
                Productos = categoria.Productos
            }).ToList();

            return Ok(ApiResponse<ICollection<CategoriaProductoResponseDto>>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las categorías de productos.");
            return StatusCode(500, ApiResponse<ICollection<CategoriaProductoResponseDto>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<ScrapingStatusResponseDto>>> GetStatus()
    {
        try
        {
            var status = await _productoService.GetScrapingStatusAsync();
            return Ok(ApiResponse<ScrapingStatusResponseDto>.Ok(Map(status)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el estado del scraping.");
            return StatusCode(500, ApiResponse<ScrapingStatusResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    private static ProductoPaginadoResponseDto Map(ProductoPaginadoResult result)
    {
        return new ProductoPaginadoResponseDto
        {
            Total = result.Total,
            Pagina = result.Pagina,
            PorPagina = result.PorPagina,
            Productos = result.Productos.Select(Map).ToList()
        };
    }

    private static ProductoResponseDto Map(ProductoResult producto)
    {
        return new ProductoResponseDto
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
            ImagenUrlOrigen = producto.ImagenUrlOrigen,
            Imagen = producto.Imagen,
            Descripcion = producto.Descripcion,
            Disponible = producto.Disponible,
            Revisar = producto.Revisar,
            PrimeraVezVisto = producto.PrimeraVezVisto,
            UltimaActualizacion = producto.UltimaActualizacion
        };
    }

    private static ScrapingStatusResponseDto Map(ScrapingStatusResult status)
    {
        return new ScrapingStatusResponseDto
        {
            TotalProductos = status.TotalProductos,
            Tiendas = status.Tiendas,
            UltimasCorridas = status.UltimasCorridas.Select(corrida => new CorridaScrapingResponseDto
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
}
