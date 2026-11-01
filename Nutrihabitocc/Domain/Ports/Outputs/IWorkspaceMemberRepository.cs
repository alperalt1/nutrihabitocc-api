using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IWorkspaceMemberRepository
{
    Task<ICollection<WorkspaceMember>> GetByWorkspaceAsync(int workspaceId);
    Task<WorkspaceMember?> GetByIdAsync(int id);
    Task<WorkspaceMember?> GetActiveByUserAndWorkspaceAsync(int userId, int workspaceId);
    Task<bool> IsActiveMemberAsync(int userId, int workspaceId, string memberRole);
    Task<ICollection<int>> GetActiveWorkspaceIdsAsync(int userId, string memberRole);
    Task<int> CountActiveByRoleAsync(int workspaceId, string memberRole);
    Task<WorkspaceMember> CreateAsync(WorkspaceMember workspaceMember);
    Task<bool> UpdateAsync(WorkspaceMember workspaceMember);
    Task<bool> DeleteAsync(int id);
}
