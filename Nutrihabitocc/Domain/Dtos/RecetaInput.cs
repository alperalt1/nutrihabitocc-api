namespace Nutrihabitocc.Domain.Dtos;

public class RecetaInput
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Instrucciones { get; set; }
    public int PorcionesBase { get; set; } = 1;
    public int? WorkspaceId { get; set; }
    public ICollection<RecetaIngredienteInput> Ingredientes { get; set; } = new List<RecetaIngredienteInput>();
}

public class RecetaIngredienteInput
{
    public long ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public string Unidad { get; set; } = "unidad";
    public string? Notas { get; set; }
}
