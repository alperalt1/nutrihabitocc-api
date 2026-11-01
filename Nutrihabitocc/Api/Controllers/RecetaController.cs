using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nutrihabitocc.Api.Dtos;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Exceptions;
using Nutrihabitocc.Domain.Ports.Inputs;

namespace Nutrihabitocc.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicies.WebStaffOnly)]
[Route("api/[controller]")]
public class RecetaController: ControllerBase
{
    private readonly INutritionPlanService _service;
    private readonly ILogger<RecetaController> _logger;

    public RecetaController(INutritionPlanService service, ILogger<RecetaController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ICollection<RecetaResult>>>> GetAll(
        [FromQuery] int? workspaceId,
        [FromQuery] string? q)
    {
        try
        {
            return Ok(ApiResponse<ICollection<RecetaResult>>.Ok(
                await _service.GetRecetasAsync(workspaceId, q)));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<RecetaResult>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las recetas.");
            return StatusCode(500, ApiResponse<ICollection<RecetaResult>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RecetaResult>>> GetById(int id)
    {
        try
        {
            var receta = await _service.GetRecetaByIdAsync(id);
            if (receta is null)
            {
                return NotFound(ApiResponse<RecetaResult>.Fail("La receta no existe."));
            }

            return Ok(ApiResponse<RecetaResult>.Ok(receta));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<RecetaResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la receta {Id}.", id);
            return StatusCode(500, ApiResponse<RecetaResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RecetaResult>>> Create([FromBody] RecetaInput input)
    {
        try
        {
            var created = await _service.CreateRecetaAsync(input);
            return Ok(ApiResponse<RecetaResult>.Ok(created, "Receta creada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<RecetaResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RecetaResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<RecetaResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la receta.");
            return StatusCode(500, ApiResponse<RecetaResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<RecetaResult>>> Update(
        int id,
        [FromBody] RecetaInput input)
    {
        try
        {
            var updated = await _service.UpdateRecetaAsync(id, input);
            return Ok(ApiResponse<RecetaResult>.Ok(updated, "Receta actualizada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<RecetaResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RecetaResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<RecetaResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la receta {Id}.", id);
            return StatusCode(500, ApiResponse<RecetaResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteRecetaAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<string>.Fail("La receta no existe."));
            }

            return Ok(ApiResponse<string>.Ok("Receta eliminada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la receta {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPost("{id:int}/copiar")]
    public async Task<ActionResult<ApiResponse<RecetaResult>>> Copy(
        int id,
        [FromBody] CopyToWorkspaceInput? input)
    {
        try
        {
            var copy = await _service.CopyRecetaAsync(id, input?.WorkspaceId);
            return Ok(ApiResponse<RecetaResult>.Ok(copy, "Receta copiada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<RecetaResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RecetaResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<RecetaResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al copiar la receta {Id}.", id);
            return StatusCode(500, ApiResponse<RecetaResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }
}
