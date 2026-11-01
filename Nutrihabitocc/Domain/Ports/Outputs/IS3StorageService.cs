using Nutrihabitocc.Domain.Dtos;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IS3StorageService
{
    string? GetPresignedUrl(string key, int expiresMinutes = 60);
    Task<S3ObjectResult?> GetObjectAsync(string key, CancellationToken cancellationToken = default);
}
