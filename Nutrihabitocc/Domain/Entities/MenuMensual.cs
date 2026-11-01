namespace Nutrihabitocc.Domain.Entities;

public class MenuMensual: Auditory
{
    public int Id { get; set; }
    public int PacienteUserId { get; set; }
    public User Paciente { get; set; } = null!;
    public int WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string Estado { get; set; } = "Borrador";
    public int? PlantillaOrigenId { get; set; }
    public string? Observaciones { get; set; }
    public int? CreadoPorUserId { get; set; }
    public int? PublicadoPorUserId { get; set; }
    public DateTime? PublicadoEn { get; set; }

    public ICollection<MenuDia> Dias { get; set; } = new List<MenuDia>();
}
