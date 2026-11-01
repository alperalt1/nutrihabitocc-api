using System.Text.Json;

namespace Nutrihabitocc.Api.Dtos.Plantilla;

public class PlantillaResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class PlantillaDetalleResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public JsonElement Estructura { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class PlantillaRequestDto
{
    public string Name { get; set; } = string.Empty;
    public JsonElement Estructura { get; set; }
    public string Version { get; set; } = string.Empty;
}
