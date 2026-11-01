namespace Nutrihabitocc.Api.Dtos.User;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public ICollection<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedOn { get; set; }
}

public class UpdateUserRolesRequestDto
{
    public ICollection<int> RoleIds { get; set; } = new List<int>();
}
