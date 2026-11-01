namespace Nutrihabitocc.Domain.Entities;

public class PrecioHistorial
{
    public long Id { get; set; }
    public long ProductoId { get; set; }
    public decimal? Precio { get; set; }
    public decimal? PrecioRegular { get; set; }
    public DateTime? Fecha { get; set; }

    public Producto Producto { get; set; } = null!;
}
