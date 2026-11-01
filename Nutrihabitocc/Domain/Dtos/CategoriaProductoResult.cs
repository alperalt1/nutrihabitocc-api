namespace Nutrihabitocc.Domain.Dtos;

public class CategoriaProductoResult
{
    public string Tienda { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string? Subcategoria { get; set; }
    public int Productos { get; set; }
}
