namespace Nutrihabitocc.Domain.Entities;

public class MenuPlantillaComida: Auditory
{
    public int Id { get; set; }
    public int MenuPlantillaDiaId { get; set; }
    public MenuPlantillaDia MenuPlantillaDia { get; set; } = null!;
    public string TipoComida { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int? RecetaId { get; set; }
    public Receta? Receta { get; set; }
    public decimal Porciones { get; set; } = 1;
    public TimeOnly? Hora { get; set; }
    public string? Observaciones { get; set; }
}
