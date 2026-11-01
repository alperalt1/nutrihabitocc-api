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
public class MenuPlantillaController: ControllerBase
{
    private readonly INutritionPlanService _service;
    private readonly ILogger<MenuPlantillaController> _logger;

    public MenuPlantillaController(
        INutritionPlanService service,
        ILogger<MenuPlantillaController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ICollection<MenuPlantillaResult>>>> GetAll(
        [FromQuery] int? workspaceId,
        [FromQuery] string? q)
    {
        try
        {
            return Ok(ApiResponse<ICollection<MenuPlantillaResult>>.Ok(
                await _service.GetPlantillasAsync(workspaceId, q)));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<MenuPlantillaResult>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las plantillas de menú.");
            return StatusCode(500, ApiResponse<ICollection<MenuPlantillaResult>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<MenuPlantillaResult>>> GetById(int id)
    {
        try
        {
            var plantilla = await _service.GetPlantillaByIdAsync(id);
            if (plantilla is null)
            {
                return NotFound(ApiResponse<MenuPlantillaResult>.Fail("La plantilla no existe."));
            }

            return Ok(ApiResponse<MenuPlantillaResult>.Ok(plantilla));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la plantilla {Id}.", id);
            return StatusCode(500, ApiResponse<MenuPlantillaResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MenuPlantillaResult>>> Create(
        [FromBody] MenuPlantillaInput input)
    {
        try
        {
            var created = await _service.CreatePlantillaAsync(input);
            return Ok(ApiResponse<MenuPlantillaResult>.Ok(
                created,
                "Plantilla de menú creada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la plantilla de menú.");
            return StatusCode(500, ApiResponse<MenuPlantillaResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<MenuPlantillaResult>>> Update(
        int id,
        [FromBody] MenuPlantillaInput input)
    {
        try
        {
            var updated = await _service.UpdatePlantillaAsync(id, input);
            return Ok(ApiResponse<MenuPlantillaResult>.Ok(
                updated,
                "Plantilla de menú actualizada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la plantilla {Id}.", id);
            return StatusCode(500, ApiResponse<MenuPlantillaResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeletePlantillaAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<string>.Fail("La plantilla no existe."));
            }

            return Ok(ApiResponse<string>.Ok("Plantilla de menú eliminada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la plantilla {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPost("{id:int}/copiar")]
    public async Task<ActionResult<ApiResponse<MenuPlantillaResult>>> Copy(
        int id,
        [FromBody] CopyToWorkspaceInput? input)
    {
        try
        {
            var copy = await _service.CopyPlantillaAsync(id, input?.WorkspaceId);
            return Ok(ApiResponse<MenuPlantillaResult>.Ok(
                copy,
                "Plantilla de menú copiada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al copiar la plantilla {Id}.", id);
            return StatusCode(500, ApiResponse<MenuPlantillaResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPost("desde-menu/{menuId:int}")]
    public async Task<ActionResult<ApiResponse<MenuPlantillaResult>>> FromMenu(
        int menuId,
        [FromBody] CreatePlantillaFromMenuInput input)
    {
        try
        {
            var created = await _service.CreatePlantillaFromMenuAsync(menuId, input);
            return Ok(ApiResponse<MenuPlantillaResult>.Ok(
                created,
                "Plantilla creada desde el menú exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuPlantillaResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la plantilla desde el menú {MenuId}.", menuId);
            return StatusCode(500, ApiResponse<MenuPlantillaResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }
}
