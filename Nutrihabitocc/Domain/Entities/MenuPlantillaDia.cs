namespace Nutrihabitocc.Domain.Entities;

public class MenuPlantillaDia: Auditory
{
    public int Id { get; set; }
    public int MenuPlantillaId { get; set; }
    public MenuPlantilla MenuPlantilla { get; set; } = null!;
    public int Dia { get; set; }
    public int MaxComidas { get; set; } = 5;
    public string? Observaciones { get; set; }

    public ICollection<MenuPlantillaComida> Comidas { get; set; } = new List<MenuPlantillaComida>();
}
