using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IAuthRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> ExistsByIdAsync(int id);
    Task SaveRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    
    Task SaveUserOtpAsync(UserOtp otp);
    Task<UserOtp?> GetValidOtpAsync(int userId, string code);
    Task UpdateUserOtpAsync(UserOtp otp);
    Task InvalidatePreviousOtpsAsync(int userId);
}
