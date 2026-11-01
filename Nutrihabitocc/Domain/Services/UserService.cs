using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Exceptions;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Domain.Services;

public class UserService: IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IAccessControlService _accessControlService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IAuthRepository authRepository,
        IAccessControlService accessControlService,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _authRepository = authRepository;
        _accessControlService = accessControlService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ICollection<RoleResult>> GetAllRolesAsync()
    {
        _accessControlService.EnsureStaffWeb();
        var currentUser = _accessControlService.GetCurrentUser();
        var roles = await _userRepository.GetRolesAsync();

        if (currentUser.IsAdmin)
        {
            return roles;
        }

        return roles
            .Where(role => string.Equals(role.RoleName, AppRoles.Paciente, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<ICollection<UserResult>> GetUsersAsync()
    {
        _accessControlService.EnsureStaffWeb();
        var currentUser = _accessControlService.GetCurrentUser();

        if (currentUser.IsAdmin)
        {
            var users = await _userRepository.GetUsersAsync();
            return users.Select(Map).ToList();
        }

        var patients = await _userRepository.GetPatientsForNutritionistAsync(currentUser.UserId);
        return patients.Select(Map).ToList();
    }

    public async Task<UserResult> CreateUserAsync(User user, string plainPassword, int roleId)
    {
        _accessControlService.EnsureStaffWeb();
        var currentUser = _accessControlService.GetCurrentUser();

        if (string.IsNullOrWhiteSpace(user.Nombres) ||
            string.IsNullOrWhiteSpace(user.Apellidos) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(plainPassword))
        {
            throw new InvalidOperationException("Nombres, apellidos, correo y contraseña son obligatorios.");
        }

        if (await _authRepository.ExistsByEmailAsync(user.Email))
        {
            throw new InvalidOperationException("El correo ya está registrado.");
        }

        var role = await _userRepository.GetRoleByIdAsync(roleId);
        if (role is null)
        {
            throw new InvalidOperationException("El rol indicado no existe o está inactivo.");
        }

        if (!currentUser.IsAdmin &&
            !string.Equals(role.Nombre, AppRoles.Paciente, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenAccessException("Un nutricionista solo puede crear usuarios con rol Paciente.");
        }

        user.Password = _passwordHasher.HashPassword(plainPassword);
        user.CreatedBy = currentUser.UserId.ToString();
        user.UserRoles.Add(new UserRole { RoleId = roleId });

        var created = await _authRepository.CreateAsync(user);
        var loaded = await _userRepository.GetByIdAsync(created.Id) ?? created;
        return Map(loaded);
    }

    public async Task<bool> UpdateUserRolesAsync(int userId, ICollection<int> roleIds)
    {
        _accessControlService.EnsureAdminWeb();

        var distinctRoleIds = roleIds.Distinct().ToList();
        if (distinctRoleIds.Count == 0)
        {
            throw new InvalidOperationException("El usuario debe tener al menos un rol.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        foreach (var roleId in distinctRoleIds)
        {
            if (await _userRepository.GetRoleByIdAsync(roleId) is null)
            {
                throw new InvalidOperationException($"El rol con Id {roleId} no existe o está inactivo.");
            }
        }

        return await _userRepository.UpdateUserRolesAsync(userId, distinctRoleIds);
    }

    public async Task<bool> CreateProfileAsync(Profile perfil)
    {
        await _accessControlService.EnsureCanManagePatientAsync(perfil.UserId);

        var existUser = await _authRepository.ExistsByIdAsync(perfil.UserId);
        if (!existUser)
        {
            throw new InvalidOperationException("El usuario no está registrado.");
        }

        return await _userRepository.CreatePerfilAsync(perfil);
    }

    public async Task<bool> EditProfileAsync(Profile perfil)
    {
        await _accessControlService.EnsureCanManagePatientAsync(perfil.UserId);

        var existProfile = await _userRepository.ExistByIdProfileAsync(perfil.Id);
        if (!existProfile)
        {
            throw new InvalidOperationException("El perfil no está registrado.");
        }

        var updated = await _userRepository.UpdateProfileAsync(perfil);
        if (!updated)
        {
            throw new InvalidOperationException("El perfil no pertenece al usuario indicado.");
        }

        return true;
    }

    private static UserResult Map(User user)
    {
        return new UserResult
        {
            Id = user.Id,
            Nombres = user.Nombres,
            Apellidos = user.Apellidos,
            Email = user.Email,
            Phone = user.Phone,
            IsActive = user.IsActive,
            CreatedOn = user.CreatedOn,
            Roles = user.UserRoles
                .Where(userRole => userRole.IsActive && userRole.Role.IsActive)
                .Select(userRole => userRole.Role.Nombre)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }
}
