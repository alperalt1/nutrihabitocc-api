namespace Nutrihabitocc.Api.Dtos.User;

public class PerfilRequestDto
{
    public int PerfilId { get; set; }
    public Double Height { get; set; }
    public Double Weight { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public Double GrasaCorporal { get; set; }
    public Double MasaMuscular { get; set; }
    public int UserId { get; set; }
}