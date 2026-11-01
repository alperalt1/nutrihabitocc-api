using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Inputs;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password, string platform);
    Task<LoginResult> RefreshTokenAsync(string refreshToken);
    Task GenerarCorreoRecuperacionAsync(string email);
    Task<bool> ResetPasswordWithOtpAsync(string email, string code, string newPassword);
}
