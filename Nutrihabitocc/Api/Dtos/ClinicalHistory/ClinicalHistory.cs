using System.Text.Json;

namespace Nutrihabitocc.Api.Dtos.ClinicalHistory;

public class ClinicalHistoryRequestDto
{
    public int UserId { get; set; }
    public int? WorkspaceId { get; set; }
    public int PlantillaFormularioId { get; set; }
    public JsonElement ContentJson { get; set; }
}

public class ClinicalHistoryContentRequestDto
{
    public JsonElement ContentJson { get; set; }
}

public class ClinicalHistoryAssignWorkspaceRequestDto
{
    public int? WorkspaceId { get; set; }
}

public class ClinicalHistoryResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public int? WorkspaceId { get; set; }
    public string? WorkspaceName { get; set; }
    public int? PlantillaFormularioId { get; set; }
    public string? PlantillaName { get; set; }
    public string? PlantillaVersion { get; set; }
    public JsonElement ContentJson { get; set; }
    public JsonElement FormularioSnapshotJson { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
