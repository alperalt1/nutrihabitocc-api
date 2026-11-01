using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Inputs;

public interface IPlantillaService
{
    Task<ICollection<PlantillaResult>> GetAllPlantillasAsync();
    Task<PlantillaDetalleResult?> GetPlantillaByIdAsync(int id);
    Task<bool> CreatePlantillaAsync(PlantillaFormulario plantillaFormulario);
}
