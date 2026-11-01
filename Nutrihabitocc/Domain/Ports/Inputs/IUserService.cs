using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Inputs;

public interface IUserService
{
    Task<ICollection<RoleResult>> GetAllRolesAsync();
    Task<ICollection<UserResult>> GetUsersAsync();
    Task<UserResult> CreateUserAsync(User user, string plainPassword, int roleId);
    Task<bool> UpdateUserRolesAsync(int userId, ICollection<int> roleIds);
    Task<bool> CreateProfileAsync(Profile perfil);
    Task<bool> EditProfileAsync(Profile perfil);
}
