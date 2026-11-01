namespace Nutrihabitocc.Domain.Dtos;

public class MenuPlantillaInput
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? WorkspaceId { get; set; }
    public ICollection<MenuPlantillaDiaInput> Dias { get; set; } = new List<MenuPlantillaDiaInput>();
}

public class MenuPlantillaDiaInput
{
    public int Dia { get; set; }
    public int MaxComidas { get; set; } = 5;
    public string? Observaciones { get; set; }
    public ICollection<MenuPlantillaComidaInput> Comidas { get; set; } = new List<MenuPlantillaComidaInput>();
}

public class MenuPlantillaComidaInput
{
    public string TipoComida { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int? RecetaId { get; set; }
    public decimal Porciones { get; set; } = 1;
    public TimeOnly? Hora { get; set; }
    public string? Observaciones { get; set; }
}

public class CopyToWorkspaceInput
{
    public int? WorkspaceId { get; set; }
}

public class CreatePlantillaFromMenuInput
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? WorkspaceId { get; set; }
}
