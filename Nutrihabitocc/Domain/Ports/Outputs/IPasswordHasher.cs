using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string password, string hashedPassword);
}