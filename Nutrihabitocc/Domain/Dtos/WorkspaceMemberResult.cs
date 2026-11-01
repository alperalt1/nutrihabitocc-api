namespace Nutrihabitocc.Domain.Dtos;

public class WorkspaceMemberResult
{
    public int Id { get; set; }
    public string MemberRole { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}
