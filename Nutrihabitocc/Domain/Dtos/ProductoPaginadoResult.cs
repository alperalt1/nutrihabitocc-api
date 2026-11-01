namespace Nutrihabitocc.Domain.Dtos;

public class ProductoPaginadoResult
{
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int PorPagina { get; set; }
    public ICollection<ProductoResult> Productos { get; set; } = new List<ProductoResult>();
}
