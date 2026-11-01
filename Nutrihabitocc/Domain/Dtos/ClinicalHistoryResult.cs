namespace Nutrihabitocc.Domain.Dtos;

public class ClinicalHistoryResult
{
    public int Id { get; set; }
    public string ContentJson { get; set; } = string.Empty;
    public string FormularioSnapshotJson { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public int? WorkspaceId { get; set; }
    public string? WorkspaceName { get; set; }
    public int? PlantillaFormularioId { get; set; }
    public string? PlantillaName { get; set; }
    public string? PlantillaVersion { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
