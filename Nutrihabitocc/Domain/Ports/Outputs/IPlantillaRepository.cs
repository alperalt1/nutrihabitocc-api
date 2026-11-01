using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface IPlantillaRepository
{
    Task<ICollection<PlantillaResult>> GetPlantillasAsync();
    Task<PlantillaFormulario?> GetByIdAsync(int id);
    Task<bool> CreatePlantillaAsync(PlantillaFormulario plantillaFormulario);
    Task<bool> ExistByIdPlantillaAsync(int id);
}
