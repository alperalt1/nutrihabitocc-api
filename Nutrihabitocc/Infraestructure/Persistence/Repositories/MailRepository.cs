using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Persistence.Repositories;

public class MailRepository: IMailRepository
{
    private readonly ILogger<MailRepository> _logger;
    private readonly IConfiguration _configuration;
    
    public  MailRepository(ILogger<MailRepository> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
        
    }
    
    public async Task EnviarCorreo(string destino, string asunto, string contenidoHtml)
    {
        var senderEmail = _configuration["Resent:From"] ?? string.Empty;
        var senderName = _configuration["Resent:FromName"] ?? string.Empty;
        var host = _configuration["Resent:Host"] ?? string.Empty;
        var user = _configuration["Resent:UserName"] ?? string.Empty;
        var password = _configuration["Resent:Password"] ?? string.Empty;
        var portString = _configuration["Resent:Port"];
        if (!int.TryParse(portString, out var port))
        {
            port = 587; 
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(senderName, senderEmail));
        email.To.Add(MailboxAddress.Parse(destino));
        
        email.Subject = asunto;
        email.Body = new TextPart(TextFormat.Html) { Text = contenidoHtml };

        using var smtp = new SmtpClient();
        smtp.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        
        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
        {
            await smtp.AuthenticateAsync(user, password);
        }
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}