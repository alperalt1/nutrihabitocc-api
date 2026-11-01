namespace Nutrihabitocc.Domain.Entities;

public class Profile: Auditory
{
    public int Id { get; set; }
    public Double Height { get; set; }
    public Double Weight { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public Double GrasaCorporal { get; set; }
    public Double MasaMuscular { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}