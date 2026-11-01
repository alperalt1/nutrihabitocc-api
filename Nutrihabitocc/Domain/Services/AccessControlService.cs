using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Exceptions;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Domain.Services;

public class AccessControlService: IAccessControlService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IUserRepository _userRepository;

    public AccessControlService(
        ICurrentUserService currentUserService,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _workspaceMemberRepository = workspaceMemberRepository;
        _userRepository = userRepository;
    }

    public CurrentUser GetCurrentUser()
    {
        return _currentUserService.GetCurrentUser();
    }

    public void EnsureAdminWeb()
    {
        var currentUser = GetCurrentUser();
        if (!currentUser.IsAdmin || !currentUser.IsWeb)
        {
            throw new ForbiddenAccessException("Esta operación requiere un administrador desde la web.");
        }
    }

    public void EnsureStaffWeb()
    {
        var currentUser = GetCurrentUser();
        if ((!currentUser.IsAdmin && !currentUser.IsNutricionista) || !currentUser.IsWeb)
        {
            throw new ForbiddenAccessException("Esta operación requiere un administrador o nutricionista desde la web.");
        }
    }

    public async Task EnsureCanViewWorkspaceAsync(int workspaceId)
    {
        var currentUser = GetCurrentUser();

        if (currentUser.IsAdmin)
        {
            if (!currentUser.IsWeb)
            {
                throw new ForbiddenAccessException("El administrador debe usar la plataforma web.");
            }

            return;
        }

        if (!currentUser.IsNutricionista || !currentUser.IsWeb)
        {
            throw new ForbiddenAccessException("No tienes acceso a este workspace.");
        }

        await EnsureNutritionistBelongsToWorkspaceAsync(currentUser.UserId, workspaceId);
    }

    public async Task EnsureCanManageWorkspaceAsync(int workspaceId)
    {
        await EnsureCanViewWorkspaceAsync(workspaceId);
    }

    public void EnsureCanManageWorkspaceMembers()
    {
        EnsureAdminWeb();
    }

    public async Task EnsureCanViewClinicalHistoryAsync(ClinicalHistory clinicalHistory)
    {
        var currentUser = GetCurrentUser();

        if (currentUser.IsAdmin)
        {
            if (!currentUser.IsWeb)
            {
                throw new ForbiddenAccessException("El administrador debe usar la plataforma web.");
            }

            return;
        }

        if (currentUser.IsNutricionista)
        {
            if (!currentUser.IsWeb || !clinicalHistory.WorkspaceId.HasValue)
            {
                throw new ForbiddenAccessException("No tienes acceso a esta historia clínica.");
            }

            await EnsureNutritionistBelongsToWorkspaceAsync(
                currentUser.UserId,
                clinicalHistory.WorkspaceId.Value);

            return;
        }

        if (currentUser.IsPaciente)
        {
            if (!currentUser.IsMobile || clinicalHistory.UserId != currentUser.UserId)
            {
                throw new ForbiddenAccessException("Solo puedes consultar tus propias historias clínicas desde la app móvil.");
            }

            return;
        }

        throw new ForbiddenAccessException("No tienes acceso a esta historia clínica.");
    }

    public async Task EnsureCanManageClinicalHistoryAsync(ClinicalHistory clinicalHistory)
    {
        var currentUser = GetCurrentUser();

        if (currentUser.IsAdmin)
        {
            if (!currentUser.IsWeb)
            {
                throw new ForbiddenAccessException("El administrador debe usar la plataforma web.");
            }

            return;
        }

        if (!currentUser.IsNutricionista || !currentUser.IsWeb || !clinicalHistory.WorkspaceId.HasValue)
        {
            throw new ForbiddenAccessException("No puedes modificar esta historia clínica.");
        }

        await EnsureNutritionistBelongsToWorkspaceAsync(
            currentUser.UserId,
            clinicalHistory.WorkspaceId.Value);
    }

    public async Task EnsureCanManageClinicalHistoryCreationAsync(int patientUserId, int? workspaceId)
    {
        var currentUser = GetCurrentUser();

        if (currentUser.IsAdmin)
        {
            if (!currentUser.IsWeb)
            {
                throw new ForbiddenAccessException("El administrador debe usar la plataforma web.");
            }

            return;
        }

        if (!currentUser.IsNutricionista || !currentUser.IsWeb)
        {
            throw new ForbiddenAccessException("Solo un nutricionista o administrador puede crear historias clínicas.");
        }

        if (!workspaceId.HasValue)
        {
            throw new InvalidOperationException("El nutricionista debe seleccionar un workspace para crear la historia clínica.");
        }

        await EnsureNutritionistBelongsToWorkspaceAsync(currentUser.UserId, workspaceId.Value);
    }

    public async Task EnsureCanViewPatientAsync(int patientUserId)
    {
        var currentUser = GetCurrentUser();

        if (currentUser.IsAdmin)
        {
            if (!currentUser.IsWeb)
            {
                throw new ForbiddenAccessException("El administrador debe usar la plataforma web.");
            }

            return;
        }

        if (currentUser.IsNutricionista)
        {
            if (!currentUser.IsWeb ||
                !await _userRepository.CanNutritionistAccessPatientAsync(currentUser.UserId, patientUserId))
            {
                throw new ForbiddenAccessException("No tienes acceso a este paciente.");
            }

            return;
        }

        if (currentUser.IsPaciente)
        {
            if (!currentUser.IsMobile || patientUserId != currentUser.UserId)
            {
                throw new ForbiddenAccessException("Solo puedes consultar tu propia información desde la app móvil.");
            }

            return;
        }

        throw new ForbiddenAccessException("No tienes acceso a este paciente.");
    }

    public Task EnsureCanManagePatientAsync(int patientUserId)
    {
        return EnsureCanViewPatientAsync(patientUserId);
    }

    private async Task EnsureNutritionistBelongsToWorkspaceAsync(int nutritionistUserId, int workspaceId)
    {
        var belongsToWorkspace = await _workspaceMemberRepository.IsActiveMemberAsync(
            nutritionistUserId,
            workspaceId,
            AppRoles.Nutricionista);

        if (!belongsToWorkspace)
        {
            throw new ForbiddenAccessException("No tienes acceso a este workspace.");
        }
    }
}
