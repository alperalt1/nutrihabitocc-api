using Nutrihabitocc.Domain.Dtos;

namespace Nutrihabitocc.Domain.Ports.Inputs;

public interface IWorkspaceService
{
    Task<ICollection<WorkspaceResult>> GetAllWorkspacesAsync();
    Task<WorkspaceResult?> GetWorkspaceByIdAsync(int id);
    Task<WorkspaceResult> CreateWorkspaceAsync(string name, string description);
    Task<bool> UpdateWorkspaceAsync(int id, string name, string description);
    Task<bool> DeleteWorkspaceAsync(int id);
}
