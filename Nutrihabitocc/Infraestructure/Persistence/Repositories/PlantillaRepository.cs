using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Persistence.Repositories;

public class PlantillaRepository: IPlantillaRepository
{
    private readonly AppDbContext _context;
    
    public  PlantillaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<PlantillaResult>> GetPlantillasAsync()
    {
        return await _context.PlantillasFormulario
            .AsNoTracking()
            .Where(r => r.IsActive)
            .Select(r => new PlantillaResult
            {
                Id = r.Id,
                Name = r.Name,
                Version = r.Version,
                IsActive = r.IsActive,
            }).ToListAsync();
    }

    public async Task<PlantillaFormulario?> GetByIdAsync(int id)
    {
        return await _context.PlantillasFormulario
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
    }

    public async Task<bool> CreatePlantillaAsync(PlantillaFormulario plantillaFormulario)
    {
        _context.PlantillasFormulario.Add(plantillaFormulario);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistByIdPlantillaAsync(int id)
    {
        return await _context.PlantillasFormulario.AnyAsync(p => p.Id == id && p.IsActive);
    }
}
