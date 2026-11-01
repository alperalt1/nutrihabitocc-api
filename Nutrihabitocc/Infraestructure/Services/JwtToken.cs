using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Services;

public class JwtToken: IJwtToken
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtToken> _logger;
    
    public JwtToken(IConfiguration configuration, ILogger<JwtToken> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateJwtToken(User user, string platform, IEnumerable<string> roles)
    {
        _logger.LogInformation("Generando JWT Token para el usuario con ID: {UserId}", user.Id);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            _logger.LogError("La clave 'Jwt:Key' no está configurada en appsettings.json.");
            throw new InvalidOperationException("Error de configuración en el servidor.");
        }
        var key = Encoding.UTF8.GetBytes(jwtKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("platform", platform)
        };

        claims.AddRange(roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            NotBefore = DateTime.UtcNow,
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        _logger.LogDebug("Token JWT generado exitosamente para {Email}", user.Email);
        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
        
    }
}
