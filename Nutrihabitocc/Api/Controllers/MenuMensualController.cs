using System.Globalization;
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
public class MenuMensualController: ControllerBase
{
    private readonly INutritionPlanService _service;
    private readonly ILogger<MenuMensualController> _logger;

    public MenuMensualController(INutritionPlanService service, ILogger<MenuMensualController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> GetById(int id)
    {
        try
        {
            var menu = await _service.GetMenuAsync(id);
            if (menu is null)
            {
                return NotFound(ApiResponse<MenuMensualResult>.Fail("El menú no existe."));
            }

            return Ok(ApiResponse<MenuMensualResult>.Ok(menu));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el menú {Id}.", id);
            return StatusCode(500, ApiResponse<MenuMensualResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpGet("paciente/{patientId:int}")]
    public async Task<ActionResult<ApiResponse<ICollection<MenuMensualResult>>>> GetByPatient(
        int patientId,
        [FromQuery] int? anio,
        [FromQuery] int? mes,
        [FromQuery] int? workspaceId)
    {
        try
        {
            return Ok(ApiResponse<ICollection<MenuMensualResult>>.Ok(
                await _service.GetMenusByPatientAsync(patientId, anio, mes, workspaceId)));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<MenuMensualResult>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los menús del paciente {PatientId}.", patientId);
            return StatusCode(500, ApiResponse<ICollection<MenuMensualResult>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> Create(
        [FromBody] MenuMensualInput input)
    {
        try
        {
            var created = await _service.CreateMenuAsync(input);
            return Ok(ApiResponse<MenuMensualResult>.Ok(created, "Menú creado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el menú mensual.");
            return StatusCode(500, ApiResponse<MenuMensualResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPost("desde-plantilla/{plantillaId:int}")]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> CreateFromPlantilla(
        int plantillaId,
        [FromBody] MenuMensualInput input)
    {
        try
        {
            var created = await _service.CreateMenuFromPlantillaAsync(plantillaId, input);
            return Ok(ApiResponse<MenuMensualResult>.Ok(
                created,
                "Menú creado desde la plantilla exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el menú desde la plantilla {PlantillaId}.", plantillaId);
            return StatusCode(500, ApiResponse<MenuMensualResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> Update(
        int id,
        [FromBody] MenuMensualUpdateInput input)
    {
        try
        {
            var updated = await _service.UpdateMenuAsync(id, input);
            return Ok(ApiResponse<MenuMensualResult>.Ok(updated, "Menú actualizado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el menú {Id}.", id);
            return StatusCode(500, ApiResponse<MenuMensualResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id:int}/dias/{fecha}")]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> UpdateDay(
        int id,
        string fecha,
        [FromBody] MenuDiaUpdateInput input)
    {
        try
        {
            var updated = await _service.UpdateMenuDayAsync(id, ParseFecha(fecha), input);
            return Ok(ApiResponse<MenuMensualResult>.Ok(updated, "Día actualizado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el día {Fecha} del menú {Id}.", fecha, id);
            return StatusCode(500, ApiResponse<MenuMensualResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPost("{id:int}/dias/{fecha}/comidas")]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> AddMeal(
        int id,
        string fecha,
        [FromBody] MenuComidaInput input)
    {
        try
        {
            var updated = await _service.AddMealAsync(id, ParseFecha(fecha), input);
            return Ok(ApiResponse<MenuMensualResult>.Ok(updated, "Comida agregada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar comida al menú {Id} para {Fecha}.", id, fecha);
            return StatusCode(500, ApiResponse<MenuMensualResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id:int}/comidas/{comidaId:int}")]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> UpdateMeal(
        int id,
        int comidaId,
        [FromBody] MenuComidaInput input)
    {
        try
        {
            var updated = await _service.UpdateMealAsync(id, comidaId, input);
            return Ok(ApiResponse<MenuMensualResult>.Ok(updated, "Comida actualizada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la comida {ComidaId} del menú {Id}.", comidaId, id);
            return StatusCode(500, ApiResponse<MenuMensualResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id:int}/comidas/{comidaId:int}")]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> DeleteMeal(int id, int comidaId)
    {
        try
        {
            var updated = await _service.DeleteMealAsync(id, comidaId);
            return Ok(ApiResponse<MenuMensualResult>.Ok(updated, "Comida eliminada exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la comida {ComidaId} del menú {Id}.", comidaId, id);
            return StatusCode(500, ApiResponse<MenuMensualResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteMenuAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<string>.Fail("El menú no existe."));
            }

            return Ok(ApiResponse<string>.Ok("Menú eliminado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el menú {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpPost("{id:int}/publicar")]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> Publish(int id)
    {
        try
        {
            var updated = await _service.PublishMenuAsync(id);
            return Ok(ApiResponse<MenuMensualResult>.Ok(updated, "Menú publicado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al publicar el menú {Id}.", id);
            return StatusCode(500, ApiResponse<MenuMensualResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    private static DateOnly ParseFecha(string fecha)
    {
        if (!DateOnly.TryParseExact(
                fecha,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
        {
            throw new InvalidOperationException("La fecha debe tener el formato yyyy-MM-dd.");
        }

        return result;
    }
}
