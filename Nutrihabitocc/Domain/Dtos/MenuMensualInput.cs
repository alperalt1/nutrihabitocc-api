namespace Nutrihabitocc.Domain.Dtos;

public class MenuMensualInput
{
    public int PacienteUserId { get; set; }
    public int WorkspaceId { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public int MaxComidasDefault { get; set; } = 5;
    public string? Observaciones { get; set; }
}

public class MenuDiaUpdateInput
{
    public int MaxComidas { get; set; }
    public string? Observaciones { get; set; }
}

public class MenuComidaInput
{
    public string TipoComida { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int RecetaId { get; set; }
    public decimal Porciones { get; set; } = 1;
    public TimeOnly? Hora { get; set; }
    public string? Observaciones { get; set; }
}

public class MenuMensualUpdateInput
{
    public string? Observaciones { get; set; }
}
