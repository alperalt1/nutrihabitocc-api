using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Inputs;

public interface IAccessControlService
{
    CurrentUser GetCurrentUser();
    void EnsureAdminWeb();
    void EnsureStaffWeb();
    Task EnsureCanViewWorkspaceAsync(int workspaceId);
    Task EnsureCanManageWorkspaceAsync(int workspaceId);
    void EnsureCanManageWorkspaceMembers();
    Task EnsureCanViewClinicalHistoryAsync(ClinicalHistory clinicalHistory);
    Task EnsureCanManageClinicalHistoryAsync(ClinicalHistory clinicalHistory);
    Task EnsureCanManageClinicalHistoryCreationAsync(int patientUserId, int? workspaceId);
    Task EnsureCanViewPatientAsync(int patientUserId);
    Task EnsureCanManagePatientAsync(int patientUserId);
}
