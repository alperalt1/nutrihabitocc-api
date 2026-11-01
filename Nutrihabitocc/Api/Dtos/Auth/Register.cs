namespace Nutrihabitocc.Api.Dtos.Auth;

public class RegisterRequestDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; }   = string.Empty;
    public string Password { get; set; }   = string.Empty;
    public string Phone { get; set; }   = string.Empty;
    public int IdRol { get; set; }
}

