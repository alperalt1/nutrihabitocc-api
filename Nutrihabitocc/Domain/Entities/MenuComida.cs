namespace Nutrihabitocc.Domain.Entities;

public class MenuComida: Auditory
{
    public int Id { get; set; }
    public int MenuDiaId { get; set; }
    public MenuDia MenuDia { get; set; } = null!;
    public string TipoComida { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int? RecetaId { get; set; }
    public Receta? Receta { get; set; }
    public string RecetaSnapshotJson { get; set; } = "{}";
    public decimal Porciones { get; set; } = 1;
    public TimeOnly? Hora { get; set; }
    public string? Observaciones { get; set; }
}
