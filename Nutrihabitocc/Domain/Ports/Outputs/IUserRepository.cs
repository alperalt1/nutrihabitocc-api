using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IUserRepository
{
    Task<ICollection<RoleResult>> GetRolesAsync();
    Task<Role?> GetRoleByIdAsync(int id);
    Task<ICollection<User>> GetUsersAsync();
    Task<ICollection<User>> GetPatientsForNutritionistAsync(int nutritionistUserId);
    Task<User?> GetByIdAsync(int id);
    Task<bool> CanNutritionistAccessPatientAsync(int nutritionistUserId, int patientUserId);
    Task<bool> UpdateUserRolesAsync(int userId, ICollection<int> roleIds);
    Task<bool> CreatePerfilAsync(Profile profile);
    Task<bool> ExistByIdProfileAsync(int id);
    Task<bool> UpdateProfileAsync(Profile profile);
    Task<bool> ExistsByIdAsync(int id);
    Task<string?> GetRoleNameAsync(string roleName);
}
