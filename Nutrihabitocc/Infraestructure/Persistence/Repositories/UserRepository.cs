using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;
    
    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<RoleResult>> GetRolesAsync()
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Where(role => role.IsActive)
            .Select(role => new RoleResult
            {
                Id = role.Id,
                RoleName = role.Nombre
            }).ToListAsync();
    }

    public async Task<Role?> GetRoleByIdAsync(int id)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(role => role.Id == id && role.IsActive);
    }

    public async Task<ICollection<User>> GetUsersAsync()
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .Where(user => user.IsActive)
            .OrderBy(user => user.Nombres)
            .ThenBy(user => user.Apellidos)
            .ToListAsync();
    }

    public async Task<ICollection<User>> GetPatientsForNutritionistAsync(int nutritionistUserId)
    {
        var createdByValue = nutritionistUserId.ToString();
        var workspaceIds = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(member =>
                member.UserId == nutritionistUserId &&
                member.IsActive &&
                member.MemberRole == AppRoles.Nutricionista)
            .Select(member => member.WorkspaceId)
            .Distinct()
            .ToListAsync();

        return await _dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .Where(user => user.IsActive)
            .Where(user => user.UserRoles.Any(userRole =>
                userRole.IsActive &&
                userRole.Role.IsActive &&
                userRole.Role.Nombre == AppRoles.Paciente))
            .Where(user =>
                user.CreatedBy == createdByValue ||
                _dbContext.WorkspaceMembers.Any(member =>
                    member.UserId == user.Id &&
                    member.IsActive &&
                    member.MemberRole == AppRoles.Paciente &&
                    workspaceIds.Contains(member.WorkspaceId)))
            .OrderBy(user => user.Nombres)
            .ThenBy(user => user.Apellidos)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Id == id && user.IsActive);
    }

    public async Task<bool> CanNutritionistAccessPatientAsync(int nutritionistUserId, int patientUserId)
    {
        var patient = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == patientUserId && user.IsActive);

        if (patient is null)
        {
            return false;
        }

        if (patient.CreatedBy == nutritionistUserId.ToString())
        {
            return true;
        }

        return await _dbContext.WorkspaceMembers.AnyAsync(nutritionistMember =>
            nutritionistMember.UserId == nutritionistUserId &&
            nutritionistMember.IsActive &&
            nutritionistMember.MemberRole == AppRoles.Nutricionista &&
            _dbContext.WorkspaceMembers.Any(patientMember =>
                patientMember.UserId == patientUserId &&
                patientMember.WorkspaceId == nutritionistMember.WorkspaceId &&
                patientMember.IsActive &&
                patientMember.MemberRole == AppRoles.Paciente));
    }

    public async Task<bool> UpdateUserRolesAsync(int userId, ICollection<int> roleIds)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user is null)
        {
            return false;
        }

        var distinctRoleIds = roleIds.Distinct().ToList();

        foreach (var userRole in user.UserRoles.ToList())
        {
            if (!distinctRoleIds.Contains(userRole.RoleId) && userRole.IsActive)
            {
                _dbContext.UserRoles.Remove(userRole);
            }
            else if (distinctRoleIds.Contains(userRole.RoleId) && !userRole.IsActive)
            {
                userRole.IsActive = true;
                userRole.DeletedBy = null;
                userRole.DeletedOn = null;
            }
        }

        foreach (var roleId in distinctRoleIds)
        {
            if (user.UserRoles.All(userRole => userRole.RoleId != roleId))
            {
                user.UserRoles.Add(new UserRole { RoleId = roleId });
            }
        }

        return await _dbContext.SaveChangesAsync() >= 0;
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await _dbContext.Users.AnyAsync(user => user.Id == id && user.IsActive);
    }

    public async Task<string?> GetRoleNameAsync(string roleName)
    {
        var normalizedRoleName = roleName.Trim().ToLower();
        return await _dbContext.Roles
            .AsNoTracking()
            .Where(role => role.IsActive && role.Nombre.ToLower() == normalizedRoleName)
            .Select(role => role.Nombre)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CreatePerfilAsync(Profile profile)
    {
        _dbContext.Profiles.Add(profile);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistByIdProfileAsync(int id)
    {
        return await _dbContext.Profiles.AnyAsync(p => p.Id == id && p.IsActive);
    }

    public async Task<bool> UpdateProfileAsync(Profile profile)
    {
        var existingProfile = await _dbContext.Profiles
            .FirstOrDefaultAsync(p => p.Id == profile.Id && p.UserId == profile.UserId && p.IsActive);
        if (existingProfile == null) return false;
        existingProfile.DateOfBirth = profile.DateOfBirth;
        existingProfile.Gender = profile.Gender;
        existingProfile.GrasaCorporal = profile.GrasaCorporal;
        existingProfile.Height = profile.Height;
        existingProfile.Weight = profile.Weight;
        existingProfile.MasaMuscular = profile.MasaMuscular;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
