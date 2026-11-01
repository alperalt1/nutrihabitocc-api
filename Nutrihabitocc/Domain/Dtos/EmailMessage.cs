namespace Nutrihabitocc.Domain.Dtos;

public class EmailMessage
{
    public string Destino { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string ContenidoHtml { get; set; } = string.Empty;
}