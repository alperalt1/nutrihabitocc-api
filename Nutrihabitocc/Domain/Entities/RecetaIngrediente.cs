namespace Nutrihabitocc.Domain.Entities;

public class RecetaIngrediente: Auditory
{
    public int Id { get; set; }
    public int RecetaId { get; set; }
    public Receta Receta { get; set; } = null!;
    public long ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public decimal Cantidad { get; set; }
    public string Unidad { get; set; } = "unidad";
    public string? Notas { get; set; }
}
