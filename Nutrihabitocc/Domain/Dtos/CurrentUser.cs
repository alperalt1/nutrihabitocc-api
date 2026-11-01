using Nutrihabitocc.Domain.Constants;

namespace Nutrihabitocc.Domain.Dtos;

public class CurrentUser
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();

    public bool IsAdmin => IsInRole(AppRoles.Administrador);
    public bool IsNutricionista => IsInRole(AppRoles.Nutricionista);
    public bool IsPaciente => IsInRole(AppRoles.Paciente);
    public bool IsWeb => string.Equals(Platform, Platforms.Web, StringComparison.OrdinalIgnoreCase);
    public bool IsMobile => string.Equals(Platform, Platforms.Mobile, StringComparison.OrdinalIgnoreCase);

    public bool IsInRole(string role)
    {
        return Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
    }
}
