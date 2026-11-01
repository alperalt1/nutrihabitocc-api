using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Inputs;

public interface IClinicalHistoryService
{
    Task<ClinicalHistoryResult?> GetClinicalHistoryByIdAsync(int id);
    Task<ICollection<ClinicalHistoryResult>> GetClinicalHistoriesAsync(int? workspaceId, int? userId);
    Task<ICollection<ClinicalHistoryResult>> GetClinicalHistoriesByWorkspaceAsync(int workspaceId);
    Task<ICollection<ClinicalHistoryResult>> GetClinicalHistoriesByPatientAsync(int userId);
    Task<ClinicalHistoryResult> CreateClinicalHistoryAsync(ClinicalHistory clinicalHistory);
    Task<bool> UpdateContentAsync(int id, string contentJson);
    Task<bool> AssignWorkspaceAsync(int id, int? workspaceId);
    Task<bool> DeleteClinicalHistoryAsync(int id);
}
