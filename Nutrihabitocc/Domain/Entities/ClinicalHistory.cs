namespace Nutrihabitocc.Domain.Entities;

public class ClinicalHistory: Auditory
{
    public int Id { get; set; }
    public string ContentJson { get; set; } = string.Empty;
    public string FormularioSnapshotJson { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int? WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }
    
    public int? PlantillaFormularioId { get; set; }
    public PlantillaFormulario? PlantillaFormulario { get; set; }
}