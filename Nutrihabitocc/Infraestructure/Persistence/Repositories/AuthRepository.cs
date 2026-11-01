using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Persistence.Repositories;

public class AuthRepository: IAuthRepository
{
    private readonly AppDbContext _context;
    
    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.IsActive);
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }
    
    public async Task SaveUserOtpAsync(UserOtp otp)
    {
        await _context.UserOtps.AddAsync(otp);
        await _context.SaveChangesAsync();
    }

    public async Task<UserOtp?> GetValidOtpAsync(int userId, string code)
    {
        return await _context.UserOtps
            .FirstOrDefaultAsync(o => o.UserId == userId 
                                      && o.Code == code 
                                      && !o.IsUsed 
                                      && o.ExpiresAt > DateTime.UtcNow 
                                      && o.IsActive);
    }

    public async Task UpdateUserOtpAsync(UserOtp otp)
    {
        _context.UserOtps.Update(otp);
        await _context.SaveChangesAsync();
    }

    public async Task InvalidatePreviousOtpsAsync(int userId)
    {
        var activeOtps = await _context.UserOtps
            .Where(o => o.UserId == userId && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow && o.IsActive)
            .ToListAsync();

        foreach (var otp in activeOtps)
        {
            otp.IsUsed = true;
        }

        if (activeOtps.Any())
        {
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id && u.IsActive);
    }
}