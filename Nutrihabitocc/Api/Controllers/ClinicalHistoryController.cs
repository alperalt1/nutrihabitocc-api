using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nutrihabitocc.Api.Dtos;
using Nutrihabitocc.Api.Dtos.ClinicalHistory;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Exceptions;
using Nutrihabitocc.Domain.Helpers;
using Nutrihabitocc.Domain.Ports.Inputs;

namespace Nutrihabitocc.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ClinicalHistoryController: ControllerBase
{
    private readonly IClinicalHistoryService _clinicalHistoryService;
    private readonly ILogger<ClinicalHistoryController> _logger;

    public ClinicalHistoryController(
        IClinicalHistoryService clinicalHistoryService,
        ILogger<ClinicalHistoryController> logger)
    {
        _clinicalHistoryService = clinicalHistoryService;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ClinicalHistoryResponseDto>>> GetById(int id)
    {
        try
        {
            var clinicalHistory = await _clinicalHistoryService.GetClinicalHistoryByIdAsync(id);
            if (clinicalHistory is null)
            {
                return NotFound(ApiResponse<ClinicalHistoryResponseDto>.Fail(
                    "La historia clínica no existe o está inactiva."));
            }

            return Ok(ApiResponse<ClinicalHistoryResponseDto>.Ok(Map(clinicalHistory)));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ClinicalHistoryResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la historia clínica {Id}.", id);
            return StatusCode(500, ApiResponse<ClinicalHistoryResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.WebStaffOnly)]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<ICollection<ClinicalHistoryResponseDto>>>> GetAll(
        [FromQuery] int? workspaceId,
        [FromQuery] int? userId)
    {
        try
        {
            var clinicalHistories = await _clinicalHistoryService
                .GetClinicalHistoriesAsync(workspaceId, userId);

            var response = clinicalHistories.Select(Map).ToList();
            return Ok(ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Ok(response));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las historias clínicas.");
            return StatusCode(500, ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.WebStaffOnly)]
    [HttpGet("workspace/{workspaceId:int}")]
    public async Task<ActionResult<ApiResponse<ICollection<ClinicalHistoryResponseDto>>>> GetByWorkspace(
        int workspaceId)
    {
        try
        {
            var clinicalHistories = await _clinicalHistoryService
                .GetClinicalHistoriesByWorkspaceAsync(workspaceId);

            var response = clinicalHistories.Select(Map).ToList();
            return Ok(ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Ok(response));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Fail(ex.Message));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las historias clínicas del workspace {WorkspaceId}.", workspaceId);
            return StatusCode(500, ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("paciente/{userId:int}")]
    public async Task<ActionResult<ApiResponse<ICollection<ClinicalHistoryResponseDto>>>> GetByPatient(
        int userId)
    {
        try
        {
            var clinicalHistories = await _clinicalHistoryService
                .GetClinicalHistoriesByPatientAsync(userId);

            var response = clinicalHistories.Select(Map).ToList();
            return Ok(ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Ok(response));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las historias clínicas del paciente {UserId}.", userId);
            return StatusCode(500, ApiResponse<ICollection<ClinicalHistoryResponseDto>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.WebStaffOnly)]
    [HttpPost("crear")]
    public async Task<ActionResult<ApiResponse<ClinicalHistoryResponseDto>>> Create(
        [FromBody] ClinicalHistoryRequestDto request)
    {
        try
        {
            var clinicalHistory = new ClinicalHistory
            {
                UserId = request.UserId,
                WorkspaceId = request.WorkspaceId,
                PlantillaFormularioId = request.PlantillaFormularioId == 0
                    ? null
                    : request.PlantillaFormularioId,
                ContentJson = JsonHelper.NormalizeRawJson(request.ContentJson, "ContentJson")
            };

            var created = await _clinicalHistoryService.CreateClinicalHistoryAsync(clinicalHistory);
            return Ok(ApiResponse<ClinicalHistoryResponseDto>.Ok(
                Map(created),
                "Historia clínica creada exitosamente."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ClinicalHistoryResponseDto>.Fail(ex.Message));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ClinicalHistoryResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ClinicalHistoryResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la historia clínica.");
            return StatusCode(500, ApiResponse<ClinicalHistoryResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.WebStaffOnly)]
    [HttpPut("{id:int}/contenido")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateContent(
        int id,
        [FromBody] ClinicalHistoryContentRequestDto request)
    {
        try
        {
            var contentJson = JsonHelper.NormalizeRawJson(request.ContentJson, "ContentJson");
            var updated = await _clinicalHistoryService.UpdateContentAsync(id, contentJson);
            if (!updated)
            {
                return NotFound(ApiResponse<string>.Fail(
                    "La historia clínica no existe o está inactiva."));
            }

            return Ok(ApiResponse<string>.Ok("Contenido de la historia clínica actualizado exitosamente."));
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
            _logger.LogError(ex, "Error al actualizar el contenido de la historia clínica {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.WebStaffOnly)]
    [HttpPut("{id:int}/asignarworkspace")]
    public async Task<ActionResult<ApiResponse<string>>> AssignWorkspace(
        int id,
        [FromBody] ClinicalHistoryAssignWorkspaceRequestDto request)
    {
        try
        {
            var assigned = await _clinicalHistoryService.AssignWorkspaceAsync(id, request.WorkspaceId);
            if (!assigned)
            {
                return NotFound(ApiResponse<string>.Fail(
                    "La historia clínica no existe o está inactiva."));
            }

            return Ok(ApiResponse<string>.Ok("Workspace asignado a la historia clínica exitosamente."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
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
            _logger.LogError(ex, "Error al asignar el workspace a la historia clínica {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.WebStaffOnly)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        try
        {
            var deleted = await _clinicalHistoryService.DeleteClinicalHistoryAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<string>.Fail(
                    "La historia clínica no existe o está inactiva."));
            }

            return Ok(ApiResponse<string>.Ok("Historia clínica eliminada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la historia clínica {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    private static ClinicalHistoryResponseDto Map(ClinicalHistoryResult clinicalHistory)
    {
        return new ClinicalHistoryResponseDto
        {
            Id = clinicalHistory.Id,
            UserId = clinicalHistory.UserId,
            PatientFullName = clinicalHistory.PatientFullName,
            WorkspaceId = clinicalHistory.WorkspaceId,
            WorkspaceName = clinicalHistory.WorkspaceName,
            PlantillaFormularioId = clinicalHistory.PlantillaFormularioId,
            PlantillaName = clinicalHistory.PlantillaName,
            PlantillaVersion = clinicalHistory.PlantillaVersion,
            ContentJson = JsonHelper.ParseElement(clinicalHistory.ContentJson),
            FormularioSnapshotJson = JsonHelper.ParseElement(clinicalHistory.FormularioSnapshotJson),
            IsActive = clinicalHistory.IsActive,
            CreatedOn = clinicalHistory.CreatedOn,
            ModifiedOn = clinicalHistory.ModifiedOn
        };
    }
}
