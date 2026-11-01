namespace Nutrihabitocc.Domain.Entities;

public class Receta: Auditory
{
    public int Id { get; set; }
    public int? WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Instrucciones { get; set; }
    public int PorcionesBase { get; set; } = 1;
    public int? RecetaOrigenId { get; set; }
    public int? CreatedByUserId { get; set; }

    public ICollection<RecetaIngrediente> Ingredientes { get; set; } = new List<RecetaIngrediente>();
}
