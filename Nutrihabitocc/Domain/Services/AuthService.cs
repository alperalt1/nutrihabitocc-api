using System.Security.Cryptography;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Exceptions;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;
using Nutrihabitocc.Infraestructure.Services;

namespace Nutrihabitocc.Domain.Services;

public class AuthService: IAuthService
{
    private readonly IJwtToken _jwtToken;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthRepository _authRepository;
    private readonly EmailQueue _emailQueue;

    public AuthService(
        IAuthRepository authRepository, 
        IPasswordHasher passwordHasher, 
        IJwtToken jwtToken,
        EmailQueue emailQueue)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _jwtToken = jwtToken;
        _emailQueue = emailQueue;
    }
    
    public async Task<LoginResult> LoginAsync(string email, string password, string platform)
    {
        var normalizedPlatform = Platforms.Normalize(platform);
        if (!Platforms.IsValid(normalizedPlatform))
        {
            throw new InvalidOperationException("La plataforma debe ser 'web' o 'mobile'.");
        }

        var user = await _authRepository.GetByEmailAsync(email);
        
        if (user == null || !_passwordHasher.VerifyHashedPassword(password, user.Password))
        {
            throw new UnauthorizedAccessException("Credenciales incorrectas.");
        }

        var roles = GetActiveRoles(user);
        ValidatePlatform(roles, normalizedPlatform);

        return await CreateSessionAsync(user, roles, normalizedPlatform);
    }

    public async Task<LoginResult> RefreshTokenAsync(string refreshToken)
    {
        var refreshTokenEntity = await _authRepository.GetRefreshTokenAsync(refreshToken);
        if (refreshTokenEntity is null || !refreshTokenEntity.IsValid)
        {
            throw new UnauthorizedAccessException("El refresh token es inválido o ha expirado.");
        }

        var user = refreshTokenEntity.User;
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("El usuario está inactivo.");
        }

        var roles = GetActiveRoles(user);
        var platform = Platforms.Normalize(refreshTokenEntity.Platform);
        if (!Platforms.IsValid(platform))
        {
            platform = Platforms.Web;
        }

        ValidatePlatform(roles, platform);

        refreshTokenEntity.IsUsed = true;
        await _authRepository.UpdateRefreshTokenAsync(refreshTokenEntity);

        return await CreateSessionAsync(user, roles, platform);
    }

    public async Task GenerarCorreoRecuperacionAsync(string email)
    {
        var user = await _authRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return;
        }
        await _authRepository.InvalidatePreviousOtpsAsync(user.Id);
        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var otpEntity = new UserOtp
        {
            Code = otpCode,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        };

        await _authRepository.SaveUserOtpAsync(otpEntity);
        var emailMessage = new EmailMessage
        {
            Destino = user.Email,
            Asunto = "Código de recuperación de contraseña - Nutrihabitocc",
            ContenidoHtml = $@"
                <h2>Recuperación de Contraseña</h2>
                <p>Tu código de verificación es:</p>
                <h1 style='letter-spacing: 5px; color: #2b2b2b;'>{otpCode}</h1>
                <p>Este código vencerá en 15 minutos.</p>"
        };

        await _emailQueue.QueueEmailAsync(emailMessage);
    }

    public async Task<bool> ResetPasswordWithOtpAsync(string email, string code, string newPassword)
    {
        var user = await _authRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        var otp = await _authRepository.GetValidOtpAsync(user.Id, code);
        if (otp == null)
        {
            return false;
        }

        otp.IsUsed = true;
        await _authRepository.UpdateUserOtpAsync(otp);
        user.Password = _passwordHasher.HashPassword(newPassword);
        return await _authRepository.UpdateUserAsync(user);
    }

    private async Task<LoginResult> CreateSessionAsync(User user, ICollection<string> roles, string platform)
    {
        var accessToken = _jwtToken.GenerateJwtToken(user, platform, roles);
        var refreshTokenValue = _jwtToken.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Platform = platform,
            IsRevoked = false,
            IsUsed = false
        };
        
        await _authRepository.SaveRefreshTokenAsync(refreshTokenEntity);
        
        return new LoginResult
        {
            User = user,
            Token = accessToken,
            RefreshToken = refreshTokenValue,
            Platform = platform,
            Roles = roles
        };
    }

    private static ICollection<string> GetActiveRoles(User user)
    {
        return user.UserRoles
            .Where(userRole => userRole.IsActive && userRole.Role.IsActive)
            .Select(userRole => userRole.Role.Nombre)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void ValidatePlatform(ICollection<string> roles, string platform)
    {
        var isStaff = roles.Any(role =>
            string.Equals(role, AppRoles.Administrador, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, AppRoles.Nutricionista, StringComparison.OrdinalIgnoreCase));

        var isPatient = roles.Any(role =>
            string.Equals(role, AppRoles.Paciente, StringComparison.OrdinalIgnoreCase));

        if (string.Equals(platform, Platforms.Mobile, StringComparison.OrdinalIgnoreCase))
        {
            if (!isPatient || isStaff)
            {
                throw new ForbiddenAccessException("La app móvil es exclusiva para pacientes.");
            }

            return;
        }

        if (string.Equals(platform, Platforms.Web, StringComparison.OrdinalIgnoreCase))
        {
            if (!isStaff)
            {
                throw new ForbiddenAccessException("La plataforma web es exclusiva para administradores y nutricionistas.");
            }

            return;
        }

        throw new InvalidOperationException("La plataforma debe ser 'web' o 'mobile'.");
    }
}
