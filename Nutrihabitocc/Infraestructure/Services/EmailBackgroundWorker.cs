using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Services;

public class EmailBackgroundWorker: BackgroundService
{
    private readonly EmailQueue _emailQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailBackgroundWorker> _logger;

    public EmailBackgroundWorker(
        EmailQueue emailQueue,
        IServiceProvider serviceProvider,
        ILogger<EmailBackgroundWorker> logger)
    {
        _emailQueue = emailQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailBackgroundWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await _emailQueue.DequeueAsync(stoppingToken);
                using var scope = _serviceProvider.CreateScope();
                var mailRepository = scope.ServiceProvider.GetRequiredService<IMailRepository>();
                _logger.LogInformation("Procesando envío de correo en segundo plano para: {Destino}", message.Destino);
                await mailRepository.EnviarCorreo(message.Destino, message.Asunto, message.ContenidoHtml);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el correo en segundo plano.");
            }
        }
    }
}