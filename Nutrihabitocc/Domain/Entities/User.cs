namespace Nutrihabitocc.Domain.Entities;

public class User : Auditory
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public Profile? Profile { get; set; }
    
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserOtp> UserOtps { get; set; } = new List<UserOtp>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<ClinicalHistory> ClinicalHistory { get; set; } = new List<ClinicalHistory>();
    public ICollection<WorkspaceMember>  WorkspaceMembers { get; set; } = new List<WorkspaceMember>();
}