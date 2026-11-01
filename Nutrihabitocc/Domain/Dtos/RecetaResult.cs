namespace Nutrihabitocc.Domain.Dtos;

public class RecetaResult
{
    public int Id { get; set; }
    public int? WorkspaceId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Instrucciones { get; set; }
    public int PorcionesBase { get; set; }
    public int? RecetaOrigenId { get; set; }
    public int? CreatedByUserId { get; set; }
    public bool IsActive { get; set; }
    public ICollection<RecetaIngredienteResult> Ingredientes { get; set; } = new List<RecetaIngredienteResult>();
}

public class RecetaIngredienteResult
{
    public int Id { get; set; }
    public long ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string? ProductoMarca { get; set; }
    public string? ProductoImagenUrl { get; set; }
    public decimal? ProductoPrecio { get; set; }
    public decimal Cantidad { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public string? Notas { get; set; }
}
