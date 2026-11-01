using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IJwtToken
{
    string GenerateJwtToken(User user, string platform, IEnumerable<string> roles);
    string GenerateRefreshToken();
}
