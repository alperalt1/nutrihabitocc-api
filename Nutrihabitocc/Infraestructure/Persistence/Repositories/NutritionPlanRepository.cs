using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Persistence.Repositories;

public class NutritionPlanRepository: INutritionPlanRepository
{
    private readonly AppDbContext _context;

    public NutritionPlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<Receta>> GetRecetasAsync(string? q)
    {
        var query = _context.Recetas
            .AsNoTracking()
            .Include(receta => receta.Workspace)
            .Include(receta => receta.Ingredientes)
            .ThenInclude(ingrediente => ingrediente.Producto)
            .Where(receta => receta.IsActive);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = $"%{q.Trim()}%";
            query = query.Where(receta => EF.Functions.ILike(receta.Nombre, term));
        }

        return await query.OrderBy(receta => receta.Nombre).ToListAsync();
    }

    public async Task<Receta?> GetRecetaByIdAsync(int id)
    {
        return await _context.Recetas
            .Include(receta => receta.Workspace)
            .Include(receta => receta.Ingredientes)
            .ThenInclude(ingrediente => ingrediente.Producto)
            .FirstOrDefaultAsync(receta => receta.Id == id && receta.IsActive);
    }

    public async Task<Receta> CreateRecetaAsync(Receta receta)
    {
        _context.Recetas.Add(receta);
        await _context.SaveChangesAsync();
        return await GetRecetaByIdAsync(receta.Id) ?? receta;
    }

    public async Task<Receta> UpdateRecetaAsync(Receta receta)
    {
        var existing = await _context.Recetas
            .Include(item => item.Ingredientes)
            .FirstOrDefaultAsync(item => item.Id == receta.Id && item.IsActive)
            ?? throw new KeyNotFoundException("La receta no existe.");

        existing.Nombre = receta.Nombre;
        existing.Descripcion = receta.Descripcion;
        existing.Instrucciones = receta.Instrucciones;
        existing.PorcionesBase = receta.PorcionesBase;

        var incoming = receta.Ingredientes.ToList();
        var incomingProductIds = incoming.Select(item => item.ProductoId).ToHashSet();

        foreach (var ingredient in existing.Ingredientes.Where(item => item.IsActive))
        {
            if (!incomingProductIds.Contains(ingredient.ProductoId))
            {
                _context.RecetaIngredientes.Remove(ingredient);
            }
        }

        foreach (var input in incoming)
        {
            var current = existing.Ingredientes
                .FirstOrDefault(item => item.IsActive && item.ProductoId == input.ProductoId);

            if (current is null)
            {
                existing.Ingredientes.Add(new RecetaIngrediente
                {
                    ProductoId = input.ProductoId,
                    Cantidad = input.Cantidad,
                    Unidad = input.Unidad,
                    Notas = input.Notas
                });
            }
            else
            {
                current.Cantidad = input.Cantidad;
                current.Unidad = input.Unidad;
                current.Notas = input.Notas;
            }
        }

        await _context.SaveChangesAsync();
        return await GetRecetaByIdAsync(existing.Id) ?? existing;
    }

    public async Task<bool> DeleteRecetaAsync(int id)
    {
        var receta = await _context.Recetas
            .Include(item => item.Ingredientes)
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

        if (receta is null)
        {
            return false;
        }

        foreach (var ingredient in receta.Ingredientes.Where(item => item.IsActive))
        {
            _context.RecetaIngredientes.Remove(ingredient);
        }

        _context.Recetas.Remove(receta);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ICollection<MenuPlantilla>> GetPlantillasAsync(string? q)
    {
        var query = _context.MenuPlantillas
            .AsNoTracking()
            .Include(plantilla => plantilla.Workspace)
            .Include(plantilla => plantilla.Dias)
            .ThenInclude(dia => dia.Comidas)
            .ThenInclude(comida => comida.Receta)
            .Where(plantilla => plantilla.IsActive);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = $"%{q.Trim()}%";
            query = query.Where(plantilla => EF.Functions.ILike(plantilla.Nombre, term));
        }

        return await query
            .AsSplitQuery()
            .OrderBy(plantilla => plantilla.Nombre)
            .ToListAsync();
    }

    public async Task<MenuPlantilla?> GetPlantillaByIdAsync(int id)
    {
        return await _context.MenuPlantillas
            .Include(plantilla => plantilla.Workspace)
            .Include(plantilla => plantilla.Dias)
            .ThenInclude(dia => dia.Comidas)
            .ThenInclude(comida => comida.Receta)
            .AsSplitQuery()
            .FirstOrDefaultAsync(plantilla => plantilla.Id == id && plantilla.IsActive);
    }

    public async Task<MenuPlantilla> CreatePlantillaAsync(MenuPlantilla plantilla)
    {
        _context.MenuPlantillas.Add(plantilla);
        await _context.SaveChangesAsync();
        return await GetPlantillaByIdAsync(plantilla.Id) ?? plantilla;
    }

    public async Task<MenuPlantilla> UpdatePlantillaAsync(MenuPlantilla plantilla)
    {
        var existing = await _context.MenuPlantillas
            .Include(item => item.Dias)
            .ThenInclude(dia => dia.Comidas)
            .AsSplitQuery()
            .FirstOrDefaultAsync(item => item.Id == plantilla.Id && item.IsActive)
            ?? throw new KeyNotFoundException("La plantilla no existe.");

        existing.Nombre = plantilla.Nombre;
        existing.Descripcion = plantilla.Descripcion;

        foreach (var dia in existing.Dias.ToList())
        {
            foreach (var comida in dia.Comidas.ToList())
            {
                _context.MenuPlantillaComidas.Remove(comida);
            }

            _context.MenuPlantillaDias.Remove(dia);
        }

        await _context.SaveChangesAsync();

        foreach (var diaInput in plantilla.Dias)
        {
            var dia = new MenuPlantillaDia
            {
                Dia = diaInput.Dia,
                MaxComidas = diaInput.MaxComidas,
                Observaciones = diaInput.Observaciones
            };

            foreach (var comidaInput in diaInput.Comidas)
            {
                dia.Comidas.Add(new MenuPlantillaComida
                {
                    TipoComida = comidaInput.TipoComida,
                    Orden = comidaInput.Orden,
                    RecetaId = comidaInput.RecetaId,
                    Porciones = comidaInput.Porciones,
                    Hora = comidaInput.Hora,
                    Observaciones = comidaInput.Observaciones
                });
            }

            existing.Dias.Add(dia);
        }

        await _context.SaveChangesAsync();
        return await GetPlantillaByIdAsync(existing.Id) ?? existing;
    }

    public async Task<bool> DeletePlantillaAsync(int id)
    {
        var plantilla = await _context.MenuPlantillas
            .Include(item => item.Dias)
            .ThenInclude(dia => dia.Comidas)
            .AsSplitQuery()
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

        if (plantilla is null)
        {
            return false;
        }

        foreach (var dia in plantilla.Dias)
        {
            foreach (var comida in dia.Comidas)
            {
                _context.MenuPlantillaComidas.Remove(comida);
            }

            _context.MenuPlantillaDias.Remove(dia);
        }

        _context.MenuPlantillas.Remove(plantilla);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MenuMensual?> GetMenuByIdAsync(int id)
    {
        return await _context.MenusMensuales
            .Include(menu => menu.Paciente)
            .Include(menu => menu.Workspace)
            .Include(menu => menu.Dias)
            .ThenInclude(dia => dia.Comidas)
            .ThenInclude(comida => comida.Receta)
            .AsSplitQuery()
            .FirstOrDefaultAsync(menu => menu.Id == id && menu.IsActive);
    }

    public async Task<MenuMensual?> GetMenuByPatientMonthAsync(
        int patientId,
        int workspaceId,
        int anio,
        int mes)
    {
        return await _context.MenusMensuales
            .AsNoTracking()
            .FirstOrDefaultAsync(menu =>
                menu.PacienteUserId == patientId &&
                menu.WorkspaceId == workspaceId &&
                menu.Anio == anio &&
                menu.Mes == mes &&
                menu.IsActive);
    }

    public async Task<MenuMensual?> GetPublishedMenuAsync(
        int patientId,
        int anio,
        int mes,
        int? workspaceId)
    {
        var query = _context.MenusMensuales
            .AsNoTracking()
            .Include(menu => menu.Paciente)
            .Include(menu => menu.Workspace)
            .Include(menu => menu.Dias)
            .ThenInclude(dia => dia.Comidas)
            .ThenInclude(comida => comida.Receta)
            .Where(menu =>
                menu.PacienteUserId == patientId &&
                menu.Anio == anio &&
                menu.Mes == mes &&
                menu.Estado == "Publicado" &&
                menu.IsActive);

        if (workspaceId.HasValue)
        {
            query = query.Where(menu => menu.WorkspaceId == workspaceId.Value);
        }

        return await query
            .AsSplitQuery()
            .OrderByDescending(menu => menu.PublicadoEn)
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<MenuMensual>> GetPublishedMenusAsync(
        int patientId,
        int anio,
        int mes)
    {
        return await _context.MenusMensuales
            .AsNoTracking()
            .Include(menu => menu.Paciente)
            .Include(menu => menu.Workspace)
            .Include(menu => menu.Dias)
            .ThenInclude(dia => dia.Comidas)
            .ThenInclude(comida => comida.Receta)
            .Where(menu =>
                menu.PacienteUserId == patientId &&
                menu.Anio == anio &&
                menu.Mes == mes &&
                menu.Estado == "Publicado" &&
                menu.IsActive)
            .AsSplitQuery()
            .OrderByDescending(menu => menu.PublicadoEn)
            .ToListAsync();
    }

    public async Task<ICollection<MenuMensual>> GetMenusByPatientAsync(
        int patientId,
        int? anio,
        int? mes,
        int? workspaceId)
    {
        var query = _context.MenusMensuales
            .AsNoTracking()
            .Include(menu => menu.Paciente)
            .Include(menu => menu.Workspace)
            .Include(menu => menu.Dias)
            .ThenInclude(dia => dia.Comidas)
            .ThenInclude(comida => comida.Receta)
            .Where(menu => menu.PacienteUserId == patientId && menu.IsActive);

        if (anio.HasValue)
        {
            query = query.Where(menu => menu.Anio == anio.Value);
        }

        if (mes.HasValue)
        {
            query = query.Where(menu => menu.Mes == mes.Value);
        }

        if (workspaceId.HasValue)
        {
            query = query.Where(menu => menu.WorkspaceId == workspaceId.Value);
        }

        return await query
            .AsSplitQuery()
            .OrderByDescending(menu => menu.Anio)
            .ThenByDescending(menu => menu.Mes)
            .ToListAsync();
    }

    public async Task<MenuMensual> CreateMenuAsync(MenuMensual menu)
    {
        _context.MenusMensuales.Add(menu);
        await _context.SaveChangesAsync();
        return await GetMenuByIdAsync(menu.Id) ?? menu;
    }

    public async Task<MenuMensual> UpdateMenuAsync(MenuMensual menu)
    {
        await _context.SaveChangesAsync();
        return await GetMenuByIdAsync(menu.Id) ?? menu;
    }

    public async Task<bool> DeleteMenuAsync(int id, string? deletedBy)
    {
        var menu = await _context.MenusMensuales
            .Include(item => item.Dias)
            .ThenInclude(day => day.Comidas)
            .AsSplitQuery()
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

        if (menu is null)
        {
            return false;
        }

        var deletedOn = DateTime.UtcNow;
        menu.IsActive = false;
        menu.DeletedOn = deletedOn;
        menu.DeletedBy = deletedBy;

        foreach (var day in menu.Dias.Where(item => item.IsActive))
        {
            day.IsActive = false;
            day.DeletedOn = deletedOn;
            day.DeletedBy = deletedBy;

            foreach (var meal in day.Comidas.Where(item => item.IsActive))
            {
                meal.IsActive = false;
                meal.DeletedOn = deletedOn;
                meal.DeletedBy = deletedBy;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MenuDia?> GetMenuDiaAsync(int menuId, DateOnly fecha)
    {
        return await _context.MenuDias
            .Include(dia => dia.Comidas)
            .ThenInclude(comida => comida.Receta)
            .FirstOrDefaultAsync(dia =>
                dia.MenuMensualId == menuId && dia.Fecha == fecha && dia.IsActive);
    }

    public async Task<MenuComida?> GetMenuComidaAsync(int menuId, int comidaId)
    {
        return await _context.MenuComidas
            .Include(comida => comida.MenuDia)
            .Include(comida => comida.Receta)
            .FirstOrDefaultAsync(comida =>
                comida.Id == comidaId &&
                comida.MenuDia.MenuMensualId == menuId &&
                comida.IsActive);
    }
}
