using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Domain.Services;

public class WorkspaceService: IWorkspaceService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IAccessControlService _accessControlService;
    private readonly ILogger<WorkspaceService> _logger;

    public WorkspaceService(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IAccessControlService accessControlService,
        ILogger<WorkspaceService> logger)
    {
        _workspaceRepository = workspaceRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _accessControlService = accessControlService;
        _logger = logger;
    }

    public async Task<ICollection<WorkspaceResult>> GetAllWorkspacesAsync()
    {
        _logger.LogInformation("GetAllWorkspaces");
        _accessControlService.EnsureStaffWeb();
        var currentUser = _accessControlService.GetCurrentUser();

        var workspaces = currentUser.IsAdmin
            ? await _workspaceRepository.GetAllAsync()
            : await _workspaceRepository.GetAllForUserAsync(currentUser.UserId);

        return workspaces.Select(Map).ToList();
    }

    public async Task<WorkspaceResult?> GetWorkspaceByIdAsync(int id)
    {
        _logger.LogInformation("GetWorkspaceById {Id}", id);
        var workspace = await _workspaceRepository.GetByIdAsync(id);
        if (workspace is null)
        {
            return null;
        }

        await _accessControlService.EnsureCanViewWorkspaceAsync(id);
        return Map(workspace);
    }

    public async Task<WorkspaceResult> CreateWorkspaceAsync(string name, string description)
    {
        _accessControlService.EnsureStaffWeb();
        ValidateName(name);

        var currentUser = _accessControlService.GetCurrentUser();
        var workspace = new Workspace
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            CreatedBy = currentUser.UserId.ToString()
        };

        var created = await _workspaceRepository.CreateAsync(workspace);

        if (!currentUser.IsAdmin && currentUser.IsNutricionista)
        {
            await _workspaceMemberRepository.CreateAsync(new WorkspaceMember
            {
                UserId = currentUser.UserId,
                WorkspaceId = created.Id,
                MemberRole = AppRoles.Nutricionista,
                CreatedBy = currentUser.UserId.ToString()
            });
        }

        return Map(created);
    }

    public async Task<bool> UpdateWorkspaceAsync(int id, string name, string description)
    {
        await _accessControlService.EnsureCanManageWorkspaceAsync(id);
        ValidateName(name);

        var workspace = new Workspace
        {
            Id = id,
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty
        };

        return await _workspaceRepository.UpdateAsync(workspace);
    }

    public async Task<bool> DeleteWorkspaceAsync(int id)
    {
        _accessControlService.EnsureAdminWeb();
        return await _workspaceRepository.DeleteAsync(id);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre del workspace es obligatorio.");
        }
    }

    private static WorkspaceResult Map(Workspace workspace)
    {
        return new WorkspaceResult
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            IsActive = workspace.IsActive,
            CreatedOn = workspace.CreatedOn
        };
    }
}
