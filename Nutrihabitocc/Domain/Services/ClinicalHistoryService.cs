using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Helpers;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Domain.Services;

public class ClinicalHistoryService: IClinicalHistoryService
{
    private readonly IClinicalHistoryRepository _clinicalHistoryRepository;
    private readonly IPlantillaRepository _plantillaRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccessControlService _accessControlService;
    private readonly ILogger<ClinicalHistoryService> _logger;

    public ClinicalHistoryService(
        IClinicalHistoryRepository clinicalHistoryRepository,
        IPlantillaRepository plantillaRepository,
        IWorkspaceRepository workspaceRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IUserRepository userRepository,
        IAccessControlService accessControlService,
        ILogger<ClinicalHistoryService> logger)
    {
        _clinicalHistoryRepository = clinicalHistoryRepository;
        _plantillaRepository = plantillaRepository;
        _workspaceRepository = workspaceRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _userRepository = userRepository;
        _accessControlService = accessControlService;
        _logger = logger;
    }

    public async Task<ClinicalHistoryResult?> GetClinicalHistoryByIdAsync(int id)
    {
        _logger.LogInformation("GetClinicalHistoryById {Id}", id);
        var clinicalHistory = await _clinicalHistoryRepository.GetByIdAsync(id);
        if (clinicalHistory is null)
        {
            return null;
        }

        await _accessControlService.EnsureCanViewClinicalHistoryAsync(clinicalHistory);
        return Map(clinicalHistory);
    }

    public async Task<ICollection<ClinicalHistoryResult>> GetClinicalHistoriesAsync(int? workspaceId, int? userId)
    {
        _logger.LogInformation("GetClinicalHistories workspaceId={WorkspaceId} userId={UserId}", workspaceId, userId);
        _accessControlService.EnsureStaffWeb();
        var currentUser = _accessControlService.GetCurrentUser();

        if (workspaceId.HasValue)
        {
            await _accessControlService.EnsureCanViewWorkspaceAsync(workspaceId.Value);
        }

        if (userId.HasValue)
        {
            await _accessControlService.EnsureCanViewPatientAsync(userId.Value);
        }

        var clinicalHistories = await _clinicalHistoryRepository.GetAllAsync(
            workspaceId,
            userId,
            currentUser.IsAdmin ? null : currentUser.UserId);

        return clinicalHistories.Select(Map).ToList();
    }

    public async Task<ICollection<ClinicalHistoryResult>> GetClinicalHistoriesByWorkspaceAsync(int workspaceId)
    {
        _logger.LogInformation("GetClinicalHistoriesByWorkspace {WorkspaceId}", workspaceId);
        await EnsureWorkspaceExistsAsync(workspaceId);
        await _accessControlService.EnsureCanViewWorkspaceAsync(workspaceId);

        var clinicalHistories = await _clinicalHistoryRepository.GetByWorkspaceAsync(workspaceId);
        return clinicalHistories.Select(Map).ToList();
    }

    public async Task<ICollection<ClinicalHistoryResult>> GetClinicalHistoriesByPatientAsync(int userId)
    {
        _logger.LogInformation("GetClinicalHistoriesByPatient {UserId}", userId);
        await EnsureUserExistsAsync(userId);
        await _accessControlService.EnsureCanViewPatientAsync(userId);

        var clinicalHistories = await _clinicalHistoryRepository.GetByPatientAsync(userId);
        return clinicalHistories.Select(Map).ToList();
    }

    public async Task<ClinicalHistoryResult> CreateClinicalHistoryAsync(ClinicalHistory clinicalHistory)
    {
        await _accessControlService.EnsureCanManageClinicalHistoryCreationAsync(
            clinicalHistory.UserId,
            clinicalHistory.WorkspaceId);

        await EnsureUserExistsAsync(clinicalHistory.UserId);

        if (!clinicalHistory.PlantillaFormularioId.HasValue)
        {
            throw new InvalidOperationException("La plantilla del formulario es obligatoria.");
        }

        var plantilla = await _plantillaRepository.GetByIdAsync(clinicalHistory.PlantillaFormularioId.Value);
        if (plantilla is null)
        {
            throw new InvalidOperationException("La plantilla no existe o está inactiva.");
        }

        clinicalHistory.ContentJson = JsonHelper.ValidateRawJson(clinicalHistory.ContentJson, "ContentJson");

        if (clinicalHistory.WorkspaceId.HasValue)
        {
            await EnsurePatientBelongsToWorkspaceAsync(clinicalHistory.UserId, clinicalHistory.WorkspaceId.Value);
        }

        clinicalHistory.FormularioSnapshotJson = plantilla.Estructura;
        clinicalHistory.CreatedBy = _accessControlService.GetCurrentUser().UserId.ToString();

        var created = await _clinicalHistoryRepository.CreateAsync(clinicalHistory);
        var loaded = await _clinicalHistoryRepository.GetByIdAsync(created.Id) ?? created;
        return Map(loaded);
    }

    public async Task<bool> UpdateContentAsync(int id, string contentJson)
    {
        var clinicalHistory = await _clinicalHistoryRepository.GetByIdAsync(id);
        if (clinicalHistory is null)
        {
            return false;
        }

        await _accessControlService.EnsureCanManageClinicalHistoryAsync(clinicalHistory);

        var normalizedContent = JsonHelper.ValidateRawJson(contentJson, "ContentJson");
        return await _clinicalHistoryRepository.UpdateContentAsync(id, normalizedContent);
    }

    public async Task<bool> AssignWorkspaceAsync(int id, int? workspaceId)
    {
        var clinicalHistory = await _clinicalHistoryRepository.GetByIdAsync(id);
        if (clinicalHistory is null)
        {
            return false;
        }

        await _accessControlService.EnsureCanManageClinicalHistoryAsync(clinicalHistory);

        if (workspaceId.HasValue)
        {
            await _accessControlService.EnsureCanManageWorkspaceAsync(workspaceId.Value);
            await EnsurePatientBelongsToWorkspaceAsync(clinicalHistory.UserId, workspaceId.Value);
        }

        return await _clinicalHistoryRepository.AssignWorkspaceAsync(id, workspaceId);
    }

    public async Task<bool> DeleteClinicalHistoryAsync(int id)
    {
        var clinicalHistory = await _clinicalHistoryRepository.GetByIdAsync(id);
        if (clinicalHistory is null)
        {
            return false;
        }

        await _accessControlService.EnsureCanManageClinicalHistoryAsync(clinicalHistory);
        return await _clinicalHistoryRepository.DeleteAsync(id);
    }

    private async Task EnsureWorkspaceExistsAsync(int workspaceId)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        if (workspace is null)
        {
            throw new KeyNotFoundException("El workspace no existe o está inactivo.");
        }
    }

    private async Task EnsureUserExistsAsync(int userId)
    {
        if (!await _userRepository.ExistsByIdAsync(userId))
        {
            throw new InvalidOperationException("El paciente no existe o está inactivo.");
        }
    }

    private async Task EnsurePatientBelongsToWorkspaceAsync(int userId, int workspaceId)
    {
        await EnsureWorkspaceExistsAsync(workspaceId);

        var member = await _workspaceMemberRepository.GetActiveByUserAndWorkspaceAsync(userId, workspaceId);
        if (member is null ||
            !string.Equals(member.MemberRole, WorkspaceRoles.Paciente, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "El paciente debe ser el miembro con rol Paciente del workspace seleccionado.");
        }
    }

    private static ClinicalHistoryResult Map(ClinicalHistory clinicalHistory)
    {
        return new ClinicalHistoryResult
        {
            Id = clinicalHistory.Id,
            ContentJson = clinicalHistory.ContentJson,
            FormularioSnapshotJson = clinicalHistory.FormularioSnapshotJson,
            UserId = clinicalHistory.UserId,
            PatientFullName = clinicalHistory.User is null
                ? string.Empty
                : $"{clinicalHistory.User.Nombres} {clinicalHistory.User.Apellidos}".Trim(),
            WorkspaceId = clinicalHistory.WorkspaceId,
            WorkspaceName = clinicalHistory.Workspace?.Name,
            PlantillaFormularioId = clinicalHistory.PlantillaFormularioId,
            PlantillaName = clinicalHistory.PlantillaFormulario?.Name,
            PlantillaVersion = clinicalHistory.PlantillaFormulario?.Version,
            IsActive = clinicalHistory.IsActive,
            CreatedOn = clinicalHistory.CreatedOn,
            ModifiedOn = clinicalHistory.ModifiedOn
        };
    }
}
