using System.Security.Claims;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Api.Services;

public class CurrentUserService: ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser GetCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("El usuario no está autenticado.");
        }

        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? principal.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("El token no contiene un usuario válido.");
        }

        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new CurrentUser
        {
            UserId = userId,
            Email = principal.FindFirstValue(ClaimTypes.Email)
                    ?? principal.FindFirstValue("email")
                    ?? string.Empty,
            Platform = principal.FindFirstValue("platform") ?? string.Empty,
            Roles = roles
        };
    }
}
