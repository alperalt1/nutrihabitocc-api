namespace Nutrihabitocc.Api.Dtos.Auth;

public class LoginRequestDto
{
    public string Email { get; set; }  = string.Empty;
    public string Password { get; set; }  = string.Empty;
    public string Platform { get; set; } = "web";
}


public class LoginResponseDto
{
    public LoginDataUser User { get; set; }  = new LoginDataUser();
    public String Token { get; set; }   = string.Empty;
    public String RefreshToken { get; set; }   = string.Empty;
    public String Platform { get; set; } = string.Empty;
    public ICollection<string> Roles { get; set; } = new List<string>();
}

public class LoginDataUser
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class MeResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public ICollection<string> Roles { get; set; } = new List<string>();
}
