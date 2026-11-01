using System.Text.Json;

namespace Nutrihabitocc.Domain.Dtos;

public class MenuMensualResult
{
    public int Id { get; set; }
    public int PacienteUserId { get; set; }
    public string PacienteNombre { get; set; } = string.Empty;
    public int WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? PlantillaOrigenId { get; set; }
    public string? Observaciones { get; set; }
    public int? CreadoPorUserId { get; set; }
    public int? PublicadoPorUserId { get; set; }
    public DateTime? PublicadoEn { get; set; }
    public bool IsActive { get; set; }
    public ICollection<MenuDiaResult> Dias { get; set; } = new List<MenuDiaResult>();
}

public class MenuDiaResult
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public int MaxComidas { get; set; }
    public string? Observaciones { get; set; }
    public ICollection<MenuComidaResult> Comidas { get; set; } = new List<MenuComidaResult>();
}

public class MenuComidaResult
{
    public int Id { get; set; }
    public string TipoComida { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int? RecetaId { get; set; }
    public string? RecetaNombre { get; set; }
    public JsonElement RecetaSnapshotJson { get; set; }
    public decimal Porciones { get; set; }
    public TimeOnly? Hora { get; set; }
    public string? Observaciones { get; set; }
}
