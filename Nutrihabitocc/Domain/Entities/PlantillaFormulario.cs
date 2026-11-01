namespace Nutrihabitocc.Domain.Entities;

public class PlantillaFormulario: Auditory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Estructura { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}