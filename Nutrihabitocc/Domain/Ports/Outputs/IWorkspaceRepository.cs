using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IWorkspaceRepository
{
    Task<ICollection<Workspace>> GetAllAsync();
    Task<ICollection<Workspace>> GetAllForUserAsync(int userId);
    Task<Workspace?> GetByIdAsync(int id);
    Task<Workspace> CreateAsync(Workspace workspace);
    Task<bool> UpdateAsync(Workspace workspace);
    Task<bool> DeleteAsync(int id);
}
