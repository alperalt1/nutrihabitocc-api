using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Inputs;

public interface IWorkspaceMemberService
{
    Task<ICollection<WorkspaceMemberResult>> GetWorkspaceMembersAsync(int workspaceId);
    Task<WorkspaceMemberResult?> GetWorkspaceMemberByIdAsync(int id);
    Task<WorkspaceMemberResult> CreateWorkspaceMemberAsync(WorkspaceMember workspaceMember);
    Task<bool> UpdateWorkspaceMemberRoleAsync(int id, string memberRole);
    Task<bool> DeleteWorkspaceMemberAsync(int id);
}
