using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Domain.Services;

public class WorkspaceMemberService: IWorkspaceMemberService
{
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccessControlService _accessControlService;
    private readonly ILogger<WorkspaceMemberService> _logger;

    public WorkspaceMemberService(
        IWorkspaceMemberRepository workspaceMemberRepository,
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository,
        IAccessControlService accessControlService,
        ILogger<WorkspaceMemberService> logger)
    {
        _workspaceMemberRepository = workspaceMemberRepository;
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
        _accessControlService = accessControlService;
        _logger = logger;
    }

    public async Task<ICollection<WorkspaceMemberResult>> GetWorkspaceMembersAsync(int workspaceId)
    {
        _logger.LogInformation("GetWorkspaceMembers {WorkspaceId}", workspaceId);
        await EnsureWorkspaceExistsAsync(workspaceId);
        await _accessControlService.EnsureCanViewWorkspaceAsync(workspaceId);
        var members = await _workspaceMemberRepository.GetByWorkspaceAsync(workspaceId);
        return members.Select(Map).ToList();
    }

    public async Task<WorkspaceMemberResult?> GetWorkspaceMemberByIdAsync(int id)
    {
        _logger.LogInformation("GetWorkspaceMemberById {Id}", id);
        var member = await _workspaceMemberRepository.GetByIdAsync(id);
        if (member is null)
        {
            return null;
        }

        await _accessControlService.EnsureCanViewWorkspaceAsync(member.WorkspaceId);
        return Map(member);
    }

    public async Task<WorkspaceMemberResult> CreateWorkspaceMemberAsync(WorkspaceMember workspaceMember)
    {
        _accessControlService.EnsureCanManageWorkspaceMembers();
        await EnsureWorkspaceExistsAsync(workspaceMember.WorkspaceId);
        await EnsureUserExistsAsync(workspaceMember.UserId);

        var memberRole = await GetCanonicalRoleAsync(workspaceMember.MemberRole);

        var existingMember = await _workspaceMemberRepository
            .GetActiveByUserAndWorkspaceAsync(workspaceMember.UserId, workspaceMember.WorkspaceId);

        if (existingMember is not null)
        {
            throw new InvalidOperationException("El usuario ya es miembro del workspace.");
        }

        if (IsPatient(memberRole))
        {
            await EnsurePatientDoesNotExistAsync(workspaceMember.WorkspaceId);
        }

        workspaceMember.MemberRole = memberRole;
        workspaceMember.CreatedBy = _accessControlService.GetCurrentUser().UserId.ToString();
        var created = await _workspaceMemberRepository.CreateAsync(workspaceMember);
        var loaded = await _workspaceMemberRepository.GetByIdAsync(created.Id) ?? created;
        return Map(loaded);
    }

    public async Task<bool> UpdateWorkspaceMemberRoleAsync(int id, string memberRole)
    {
        _accessControlService.EnsureCanManageWorkspaceMembers();
        var member = await _workspaceMemberRepository.GetByIdAsync(id);
        if (member is null)
        {
            return false;
        }

        var canonicalRole = await GetCanonicalRoleAsync(memberRole);

        if (IsPatient(canonicalRole) && !IsPatient(member.MemberRole))
        {
            await EnsurePatientDoesNotExistAsync(member.WorkspaceId);
        }

        member.MemberRole = canonicalRole;
        return await _workspaceMemberRepository.UpdateAsync(member);
    }

    public Task<bool> DeleteWorkspaceMemberAsync(int id)
    {
        _accessControlService.EnsureCanManageWorkspaceMembers();
        return _workspaceMemberRepository.DeleteAsync(id);
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
            throw new InvalidOperationException("El usuario no existe o está inactivo.");
        }
    }

    private async Task<string> GetCanonicalRoleAsync(string memberRole)
    {
        if (string.IsNullOrWhiteSpace(memberRole))
        {
            throw new InvalidOperationException("El rol del miembro es obligatorio.");
        }

        var canonicalRole = await _userRepository.GetRoleNameAsync(memberRole.Trim());
        if (canonicalRole is null ||
            (!string.Equals(canonicalRole, WorkspaceRoles.Paciente, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(canonicalRole, WorkspaceRoles.Nutricionista, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"El rol '{memberRole}' no es válido. Roles permitidos: {WorkspaceRoles.Paciente}, {WorkspaceRoles.Nutricionista}.");
        }

        return canonicalRole;
    }

    private async Task EnsurePatientDoesNotExistAsync(int workspaceId)
    {
        var patientCount = await _workspaceMemberRepository
            .CountActiveByRoleAsync(workspaceId, WorkspaceRoles.Paciente);

        if (patientCount > 0)
        {
            throw new InvalidOperationException("El workspace ya tiene un paciente asignado.");
        }
    }

    private static bool IsPatient(string memberRole)
    {
        return string.Equals(memberRole, WorkspaceRoles.Paciente, StringComparison.OrdinalIgnoreCase);
    }

    private static WorkspaceMemberResult Map(WorkspaceMember member)
    {
        return new WorkspaceMemberResult
        {
            Id = member.Id,
            MemberRole = member.MemberRole,
            UserId = member.UserId,
            UserFullName = member.User is null
                ? string.Empty
                : $"{member.User.Nombres} {member.User.Apellidos}".Trim(),
            UserEmail = member.User?.Email ?? string.Empty,
            WorkspaceId = member.WorkspaceId,
            WorkspaceName = member.Workspace?.Name ?? string.Empty,
            IsActive = member.IsActive,
            CreatedOn = member.CreatedOn
        };
    }
}
