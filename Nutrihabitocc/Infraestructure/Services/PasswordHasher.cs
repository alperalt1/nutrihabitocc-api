using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Services;

public class PasswordHasher: IPasswordHasher
{
    private readonly IConfiguration _configuration;

    public PasswordHasher(ILogger<PasswordHasher> logger, IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyHashedPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}