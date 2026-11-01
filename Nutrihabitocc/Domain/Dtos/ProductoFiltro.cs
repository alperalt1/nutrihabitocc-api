namespace Nutrihabitocc.Domain.Dtos;

public class ProductoFiltro
{
    public string? Tienda { get; set; }
    public string? Categoria { get; set; }
    public string? Subcategoria { get; set; }
    public string? Q { get; set; }
    public bool SoloDisponibles { get; set; } = true;
    public int Pagina { get; set; } = 1;
    public int PorPagina { get; set; } = 50;
}
