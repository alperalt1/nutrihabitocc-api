namespace Nutrihabitocc.Domain.Entities;

public class MenuDia: Auditory
{
    public int Id { get; set; }
    public int MenuMensualId { get; set; }
    public MenuMensual MenuMensual { get; set; } = null!;
    public DateOnly Fecha { get; set; }
    public int MaxComidas { get; set; } = 5;
    public string? Observaciones { get; set; }

    public ICollection<MenuComida> Comidas { get; set; } = new List<MenuComida>();
}
