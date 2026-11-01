using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nutrihabitocc.Api.Dtos;
using Nutrihabitocc.Api.Dtos.WorkspaceMember;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Exceptions;
using Nutrihabitocc.Domain.Ports.Inputs;

namespace Nutrihabitocc.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicies.WebStaffOnly)]
[Route("api/[controller]")]
public class WorkspaceMemberController: ControllerBase
{
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly ILogger<WorkspaceMemberController> _logger;

    public WorkspaceMemberController(
        IWorkspaceMemberService workspaceMemberService,
        ILogger<WorkspaceMemberController> logger)
    {
        _workspaceMemberService = workspaceMemberService;
        _logger = logger;
    }

    [HttpGet("workspace/{workspaceId:int}")]
    public async Task<ActionResult<ApiResponse<ICollection<WorkspaceMemberResponseDto>>>> GetByWorkspace(
        int workspaceId)
    {
        try
        {
            var members = await _workspaceMemberService.GetWorkspaceMembersAsync(workspaceId);
            var response = members.Select(Map).ToList();
            return Ok(ApiResponse<ICollection<WorkspaceMemberResponseDto>>.Ok(response));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ICollection<WorkspaceMemberResponseDto>>.Fail(ex.Message));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<WorkspaceMemberResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los miembros del workspace {WorkspaceId}.", workspaceId);
            return StatusCode(500, ApiResponse<ICollection<WorkspaceMemberResponseDto>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<WorkspaceMemberResponseDto>>> GetById(int id)
    {
        try
        {
            var member = await _workspaceMemberService.GetWorkspaceMemberByIdAsync(id);
            if (member is null)
            {
                return NotFound(ApiResponse<WorkspaceMemberResponseDto>.Fail(
                    "El miembro no existe o está inactivo."));
            }

            return Ok(ApiResponse<WorkspaceMemberResponseDto>.Ok(Map(member)));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<WorkspaceMemberResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el miembro {Id}.", id);
            return StatusCode(500, ApiResponse<WorkspaceMemberResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.AdminWebOnly)]
    [HttpPost("crear")]
    public async Task<ActionResult<ApiResponse<WorkspaceMemberResponseDto>>> Create(
        [FromBody] WorkspaceMemberRequestDto request)
    {
        try
        {
            var workspaceMember = new WorkspaceMember
            {
                UserId = request.UserId,
                WorkspaceId = request.WorkspaceId,
                MemberRole = request.MemberRole
            };

            var created = await _workspaceMemberService.CreateWorkspaceMemberAsync(workspaceMember);
            return Ok(ApiResponse<WorkspaceMemberResponseDto>.Ok(
                Map(created),
                "Miembro agregado al workspace exitosamente."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<WorkspaceMemberResponseDto>.Fail(ex.Message));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<WorkspaceMemberResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<WorkspaceMemberResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el miembro del workspace.");
            return StatusCode(500, ApiResponse<WorkspaceMemberResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.AdminWebOnly)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateRole(
        int id,
        [FromBody] WorkspaceMemberUpdateRequestDto request)
    {
        try
        {
            var updated = await _workspaceMemberService.UpdateWorkspaceMemberRoleAsync(id, request.MemberRole);
            if (!updated)
            {
                return NotFound(ApiResponse<string>.Fail("El miembro no existe o está inactivo."));
            }

            return Ok(ApiResponse<string>.Ok("Rol del miembro actualizado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el rol del miembro {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.AdminWebOnly)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        try
        {
            var deleted = await _workspaceMemberService.DeleteWorkspaceMemberAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<string>.Fail("El miembro no existe o está inactivo."));
            }

            return Ok(ApiResponse<string>.Ok("Miembro eliminado del workspace exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el miembro {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    private static WorkspaceMemberResponseDto Map(WorkspaceMemberResult member)
    {
        return new WorkspaceMemberResponseDto
        {
            Id = member.Id,
            UserId = member.UserId,
            UserFullName = member.UserFullName,
            UserEmail = member.UserEmail,
            WorkspaceId = member.WorkspaceId,
            WorkspaceName = member.WorkspaceName,
            MemberRole = member.MemberRole,
            IsActive = member.IsActive,
            CreatedOn = member.CreatedOn
        };
    }
}
