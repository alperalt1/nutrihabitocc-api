namespace Nutrihabitocc.Domain.Entities;

public class Workspace: Auditory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public ICollection<ClinicalHistory> ClinicalHistory { get; set; } = new List<ClinicalHistory>();
    public ICollection<WorkspaceMember> WorkspaceMembers { get; set; } = new List<WorkspaceMember>();
}