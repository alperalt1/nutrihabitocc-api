using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Helpers;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Domain.Services;

public class PlantillaService: IPlantillaService
{
    private readonly IPlantillaRepository _plantillaRepository;
    private readonly ILogger<PlantillaService> _logger;

    public PlantillaService(IPlantillaRepository plantillaRepository, ILogger<PlantillaService> logger)
    {
        _plantillaRepository = plantillaRepository;
        _logger = logger;
    }

    public Task<ICollection<PlantillaResult>> GetAllPlantillasAsync()
    {
        _logger.LogInformation("GetAllPlantillas");
        return _plantillaRepository.GetPlantillasAsync();
    }

    public async Task<PlantillaDetalleResult?> GetPlantillaByIdAsync(int id)
    {
        _logger.LogInformation("GetPlantillaById {Id}", id);
        var plantilla = await _plantillaRepository.GetByIdAsync(id);
        if (plantilla is null)
        {
            return null;
        }

        return new PlantillaDetalleResult
        {
            Id = plantilla.Id,
            Name = plantilla.Name,
            Version = plantilla.Version,
            Estructura = plantilla.Estructura,
            IsActive = plantilla.IsActive,
            CreatedOn = plantilla.CreatedOn
        };
    }

    public async Task<bool> CreatePlantillaAsync(PlantillaFormulario plantillaFormulario)
    {
        if (string.IsNullOrWhiteSpace(plantillaFormulario.Name))
        {
            throw new InvalidOperationException("El nombre de la plantilla es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(plantillaFormulario.Version))
        {
            throw new InvalidOperationException("La versión de la plantilla es obligatoria.");
        }

        plantillaFormulario.Estructura = JsonHelper.NormalizeStructureJson(plantillaFormulario.Estructura);
        return await _plantillaRepository.CreatePlantillaAsync(plantillaFormulario);
    }
}
