using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nutrihabitocc.Api.Dtos;
using Nutrihabitocc.Api.Dtos.Plantilla;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Helpers;
using Nutrihabitocc.Domain.Ports.Inputs;

namespace Nutrihabitocc.Api.Controllers;
[ApiController]
[Authorize(Policy = AuthPolicies.WebStaffOnly)]
[Route("api/[controller]")]
public class PlantillaController: ControllerBase
{
    private readonly IPlantillaService _plantillaService;
    private readonly ILogger<PlantillaController> _logger;

    public PlantillaController(IPlantillaService plantillaService, ILogger<PlantillaController> logger)
    {
        _plantillaService = plantillaService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<ApiResponse<ICollection<PlantillaResponseDto>>>> GetAllPlantillas()
    {
        try
        {
            var plantillas = await _plantillaService.GetAllPlantillasAsync();
            var plantillasresponse = new List<PlantillaResponseDto>();
            foreach (var plantilla in plantillas)
            {
                var responseT = new PlantillaResponseDto
                {
                    Id = plantilla.Id,
                    Name = plantilla.Name,
                    Version = plantilla.Version
                };
                plantillasresponse.Add(responseT);
            };
            return Ok(ApiResponse<ICollection<PlantillaResponseDto>>.Ok(plantillasresponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las plantillas.");
            return StatusCode(500, ApiResponse<ICollection<PlantillaResponseDto>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PlantillaDetalleResponseDto>>> GetPlantillaById(int id)
    {
        try
        {
            var plantilla = await _plantillaService.GetPlantillaByIdAsync(id);
            if (plantilla is null)
            {
                return NotFound(ApiResponse<PlantillaDetalleResponseDto>.Fail(
                    "La plantilla no existe o está inactiva."));
            }

            var response = new PlantillaDetalleResponseDto
            {
                Id = plantilla.Id,
                Name = plantilla.Name,
                Version = plantilla.Version,
                Estructura = JsonHelper.ParseElement(plantilla.Estructura),
                IsActive = plantilla.IsActive,
                CreatedOn = plantilla.CreatedOn
            };

            return Ok(ApiResponse<PlantillaDetalleResponseDto>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la plantilla {Id}.", id);
            return StatusCode(500, ApiResponse<PlantillaDetalleResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.AdminWebOnly)]
    [HttpPost("crearplantilla")]
    public async Task<ActionResult<ApiResponse<string>>> CrearPlantillaAsync([FromBody] PlantillaRequestDto plantillaFormulario)
    {
        try
        {
            var newbody = new PlantillaFormulario
            {
                Name = plantillaFormulario.Name,
                Version = plantillaFormulario.Version,
                Estructura = JsonHelper.NormalizeRawJson(plantillaFormulario.Estructura, "Estructura")
            };
            await _plantillaService.CreatePlantillaAsync(newbody);
            return Ok(ApiResponse<string>.Ok("Plantilla creada exitosamente."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la plantilla.");
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }
}
