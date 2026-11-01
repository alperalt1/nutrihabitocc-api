using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nutrihabitocc.Api.Dtos;
using Nutrihabitocc.Api.Dtos.Workspace;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Exceptions;
using Nutrihabitocc.Domain.Ports.Inputs;

namespace Nutrihabitocc.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicies.WebStaffOnly)]
[Route("api/[controller]")]
public class WorkspaceController: ControllerBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly ILogger<WorkspaceController> _logger;

    public WorkspaceController(IWorkspaceService workspaceService, ILogger<WorkspaceController> logger)
    {
        _workspaceService = workspaceService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ICollection<WorkspaceResponseDto>>>> GetAllWorkspaces()
    {
        try
        {
            var workspaces = await _workspaceService.GetAllWorkspacesAsync();
            var response = workspaces.Select(Map).ToList();
            return Ok(ApiResponse<ICollection<WorkspaceResponseDto>>.Ok(response));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<WorkspaceResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los workspaces.");
            return StatusCode(500, ApiResponse<ICollection<WorkspaceResponseDto>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<WorkspaceResponseDto>>> GetWorkspaceById(int id)
    {
        try
        {
            var workspace = await _workspaceService.GetWorkspaceByIdAsync(id);
            if (workspace is null)
            {
                return NotFound(ApiResponse<WorkspaceResponseDto>.Fail(
                    "El workspace no existe o está inactivo."));
            }

            return Ok(ApiResponse<WorkspaceResponseDto>.Ok(Map(workspace)));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<WorkspaceResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el workspace {Id}.", id);
            return StatusCode(500, ApiResponse<WorkspaceResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpPost("crear")]
    public async Task<ActionResult<ApiResponse<WorkspaceResponseDto>>> CreateWorkspace(
        [FromBody] WorkspaceRequestDto request)
    {
        try
        {
            var workspace = await _workspaceService.CreateWorkspaceAsync(request.Name, request.Description);
            return Ok(ApiResponse<WorkspaceResponseDto>.Ok(
                Map(workspace),
                "Workspace creado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<WorkspaceResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<WorkspaceResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el workspace.");
            return StatusCode(500, ApiResponse<WorkspaceResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateWorkspace(
        int id,
        [FromBody] WorkspaceRequestDto request)
    {
        try
        {
            var updated = await _workspaceService.UpdateWorkspaceAsync(id, request.Name, request.Description);
            if (!updated)
            {
                return NotFound(ApiResponse<string>.Fail("El workspace no existe o está inactivo."));
            }

            return Ok(ApiResponse<string>.Ok("Workspace actualizado exitosamente."));
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
            _logger.LogError(ex, "Error al actualizar el workspace {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.AdminWebOnly)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteWorkspace(int id)
    {
        try
        {
            var deleted = await _workspaceService.DeleteWorkspaceAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<string>.Fail("El workspace no existe o está inactivo."));
            }

            return Ok(ApiResponse<string>.Ok("Workspace eliminado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el workspace {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    private static WorkspaceResponseDto Map(WorkspaceResult workspace)
    {
        return new WorkspaceResponseDto
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            IsActive = workspace.IsActive,
            CreatedOn = workspace.CreatedOn
        };
    }
}
