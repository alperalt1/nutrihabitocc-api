using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Dtos;

public class LoginResult
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; }  = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public ICollection<string> Roles { get; set; } = new List<string>();
    public User User { get; set; } = new User();
}
