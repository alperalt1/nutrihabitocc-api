using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IClinicalHistoryRepository
{
    Task<ICollection<ClinicalHistory>> GetAllAsync(int? workspaceId, int? userId, int? nutritionistUserId);
    Task<ICollection<ClinicalHistory>> GetByWorkspaceAsync(int workspaceId);
    Task<ICollection<ClinicalHistory>> GetByPatientAsync(int userId);
    Task<ClinicalHistory?> GetByIdAsync(int id);
    Task<ClinicalHistory> CreateAsync(ClinicalHistory clinicalHistory);
    Task<bool> UpdateContentAsync(int id, string contentJson);
    Task<bool> AssignWorkspaceAsync(int id, int? workspaceId);
    Task<bool> DeleteAsync(int id);
}
