namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IMailRepository
{
    Task EnviarCorreo(string destino, string asunto,string contenidoHtml);
}