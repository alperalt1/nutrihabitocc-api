namespace Nutrihabitocc.Domain.Dtos;

public class PlantillaResult
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}