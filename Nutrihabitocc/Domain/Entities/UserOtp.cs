namespace Nutrihabitocc.Domain.Entities;

public class UserOtp: Auditory
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt && IsActive;
}