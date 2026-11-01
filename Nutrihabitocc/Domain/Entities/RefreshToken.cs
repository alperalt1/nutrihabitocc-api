namespace Nutrihabitocc.Domain.Entities;

public class RefreshToken : Auditory
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    public string Platform { get; set; } = "web";

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsValid => !IsRevoked && !IsUsed && DateTime.UtcNow < ExpiresAt && IsActive;
}
