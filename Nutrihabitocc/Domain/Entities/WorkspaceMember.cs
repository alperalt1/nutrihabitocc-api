namespace Nutrihabitocc.Domain.Entities;

public class WorkspaceMember: Auditory
{
    public int Id { get; set; }
    public string MemberRole { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }
}