namespace Nutrihabitocc.Domain.Entities;

public class MenuPlantilla: Auditory
{
    public int Id { get; set; }
    public int? WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? PlantillaOrigenId { get; set; }
    public int? CreatedByUserId { get; set; }

    public ICollection<MenuPlantillaDia> Dias { get; set; } = new List<MenuPlantillaDia>();
}
