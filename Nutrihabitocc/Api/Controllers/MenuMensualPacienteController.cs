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
[Authorize(Policy = AuthPolicies.MobilePatientOnly)]
[Route("api/MenuMensual")]
public class MenuMensualPacienteController: ControllerBase
{
    private readonly INutritionPlanService _service;
    private readonly ILogger<MenuMensualPacienteController> _logger;

    public MenuMensualPacienteController(
        INutritionPlanService service,
        ILogger<MenuMensualPacienteController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("mio")]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> GetMyMenu(
        [FromQuery] int anio,
        [FromQuery] int mes,
        [FromQuery] int? workspaceId)
    {
        try
        {
            var menu = await _service.GetMyMenuAsync(anio, mes, workspaceId);
            if (menu is null)
            {
                return NotFound(ApiResponse<MenuMensualResult>.Fail(
                    "No tienes un menú publicado para ese mes."));
            }

            return Ok(ApiResponse<MenuMensualResult>.Ok(menu));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el menú del paciente.");
            return StatusCode(500, ApiResponse<MenuMensualResult>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpGet("mio/menus")]
    public async Task<ActionResult<ApiResponse<ICollection<MenuMensualResult>>>> GetMyMenus(
        [FromQuery] int anio,
        [FromQuery] int mes)
    {
        try
        {
            return Ok(ApiResponse<ICollection<MenuMensualResult>>.Ok(
                await _service.GetMyMenusAsync(anio, mes)));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<MenuMensualResult>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los menús publicados del paciente.");
            return StatusCode(500, ApiResponse<ICollection<MenuMensualResult>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [HttpGet("mio/dias/{fecha}")]
    public async Task<ActionResult<ApiResponse<MenuMensualResult>>> GetMyMenuDay(
        string fecha,
        [FromQuery] int? workspaceId)
    {
        try
        {
            var date = ParseFecha(fecha);
            var menu = await _service.GetMyMenuDayAsync(date, workspaceId);
            if (menu is null)
            {
                return NotFound(ApiResponse<MenuMensualResult>.Fail(
                    "No tienes un menú publicado para esa fecha."));
            }

            return Ok(ApiResponse<MenuMensualResult>.Ok(menu));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuMensualResult>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el día {Fecha} del menú del paciente.", fecha);
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
