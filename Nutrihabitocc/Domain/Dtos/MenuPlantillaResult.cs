namespace Nutrihabitocc.Domain.Dtos;

public class MenuPlantillaResult
{
    public int Id { get; set; }
    public int? WorkspaceId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? PlantillaOrigenId { get; set; }
    public int? CreatedByUserId { get; set; }
    public bool IsActive { get; set; }
    public ICollection<MenuPlantillaDiaResult> Dias { get; set; } = new List<MenuPlantillaDiaResult>();
}

public class MenuPlantillaDiaResult
{
    public int Id { get; set; }
    public int Dia { get; set; }
    public int MaxComidas { get; set; }
    public string? Observaciones { get; set; }
    public ICollection<MenuPlantillaComidaResult> Comidas { get; set; } = new List<MenuPlantillaComidaResult>();
}

public class MenuPlantillaComidaResult
{
    public int Id { get; set; }
    public string TipoComida { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int? RecetaId { get; set; }
    public string? RecetaNombre { get; set; }
    public decimal Porciones { get; set; }
    public TimeOnly? Hora { get; set; }
    public string? Observaciones { get; set; }
}
