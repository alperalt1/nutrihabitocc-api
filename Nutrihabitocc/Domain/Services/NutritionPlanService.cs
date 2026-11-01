using System.Text.Json;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Exceptions;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Domain.Services;

public class NutritionPlanService: INutritionPlanService
{
    private readonly INutritionPlanRepository _repository;
    private readonly IAccessControlService _accessControl;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ILogger<NutritionPlanService> _logger;

    public NutritionPlanService(
        INutritionPlanRepository repository,
        IAccessControlService accessControl,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IWorkspaceRepository workspaceRepository,
        ILogger<NutritionPlanService> logger)
    {
        _repository = repository;
        _accessControl = accessControl;
        _workspaceMemberRepository = workspaceMemberRepository;
        _workspaceRepository = workspaceRepository;
        _logger = logger;
    }

    public async Task<ICollection<RecetaResult>> GetRecetasAsync(int? workspaceId, string? q)
    {
        _accessControl.EnsureStaffWeb();

        var recetas = await _repository.GetRecetasAsync(q);
        return recetas.Select(MapReceta).ToList();
    }

    public async Task<RecetaResult?> GetRecetaByIdAsync(int id)
    {
        _accessControl.EnsureStaffWeb();
        var receta = await _repository.GetRecetaByIdAsync(id);
        if (receta is null)
        {
            return null;
        }

        await EnsureCanViewRecetaAsync(receta);
        return MapReceta(receta);
    }

    public async Task<RecetaResult> CreateRecetaAsync(RecetaInput input)
    {
        _accessControl.EnsureStaffWeb();
        var current = _accessControl.GetCurrentUser();
        ValidateRecetaInput(input);

        var receta = new Receta
        {
            WorkspaceId = null,
            Nombre = input.Nombre.Trim(),
            Descripcion = input.Descripcion?.Trim(),
            Instrucciones = input.Instrucciones?.Trim(),
            PorcionesBase = input.PorcionesBase,
            CreatedByUserId = current.UserId,
            CreatedBy = current.UserId.ToString(),
            Ingredientes = input.Ingredientes.Select(item => new RecetaIngrediente
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                Unidad = item.Unidad.Trim(),
                Notas = item.Notas?.Trim()
            }).ToList()
        };

        var created = await _repository.CreateRecetaAsync(receta);
        return MapReceta(created);
    }

    public async Task<RecetaResult> UpdateRecetaAsync(int id, RecetaInput input)
    {
        _accessControl.EnsureStaffWeb();
        var receta = await _repository.GetRecetaByIdAsync(id)
                     ?? throw new KeyNotFoundException("La receta no existe.");
        await EnsureCanEditRecetaAsync(receta);
        ValidateRecetaInput(input);

        receta.Nombre = input.Nombre.Trim();
        receta.Descripcion = input.Descripcion?.Trim();
        receta.Instrucciones = input.Instrucciones?.Trim();
        receta.PorcionesBase = input.PorcionesBase;
        receta.Ingredientes = input.Ingredientes.Select(item => new RecetaIngrediente
        {
            ProductoId = item.ProductoId,
            Cantidad = item.Cantidad,
            Unidad = item.Unidad.Trim(),
            Notas = item.Notas?.Trim()
        }).ToList();

        var updated = await _repository.UpdateRecetaAsync(receta);
        return MapReceta(updated);
    }

    public async Task<bool> DeleteRecetaAsync(int id)
    {
        _accessControl.EnsureStaffWeb();
        var receta = await _repository.GetRecetaByIdAsync(id);
        if (receta is null)
        {
            return false;
        }

        await EnsureCanEditRecetaAsync(receta);
        return await _repository.DeleteRecetaAsync(id);
    }

    public async Task<RecetaResult> CopyRecetaAsync(int id, int? workspaceId)
    {
        _accessControl.EnsureStaffWeb();
        var current = _accessControl.GetCurrentUser();
        var original = await _repository.GetRecetaByIdAsync(id)
                       ?? throw new KeyNotFoundException("La receta no existe.");
        await EnsureCanViewRecetaAsync(original);

        var copy = new Receta
        {
            WorkspaceId = null,
            Nombre = original.Nombre,
            Descripcion = original.Descripcion,
            Instrucciones = original.Instrucciones,
            PorcionesBase = original.PorcionesBase,
            RecetaOrigenId = original.Id,
            CreatedByUserId = current.UserId,
            CreatedBy = current.UserId.ToString(),
            Ingredientes = original.Ingredientes.Select(item => new RecetaIngrediente
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                Unidad = item.Unidad,
                Notas = item.Notas
            }).ToList()
        };

        var created = await _repository.CreateRecetaAsync(copy);
        return MapReceta(created);
    }

    public async Task<ICollection<MenuPlantillaResult>> GetPlantillasAsync(int? workspaceId, string? q)
    {
        _accessControl.EnsureStaffWeb();
        var plantillas = await _repository.GetPlantillasAsync(q);
        return plantillas.Select(MapPlantilla).ToList();
    }

    public async Task<MenuPlantillaResult?> GetPlantillaByIdAsync(int id)
    {
        _accessControl.EnsureStaffWeb();
        var plantilla = await _repository.GetPlantillaByIdAsync(id);
        if (plantilla is null)
        {
            return null;
        }

        await EnsureCanViewPlantillaAsync(plantilla);
        return MapPlantilla(plantilla);
    }

    public async Task<MenuPlantillaResult> CreatePlantillaAsync(MenuPlantillaInput input)
    {
        _accessControl.EnsureStaffWeb();
        var current = _accessControl.GetCurrentUser();
        ValidatePlantillaInput(input);
        await ValidateTemplatesRecipesAsync(input);

        var plantilla = new MenuPlantilla
        {
            WorkspaceId = null,
            Nombre = input.Nombre.Trim(),
            Descripcion = input.Descripcion?.Trim(),
            CreatedByUserId = current.UserId,
            CreatedBy = current.UserId.ToString(),
            Dias = input.Dias.Select(dia => new MenuPlantillaDia
            {
                Dia = dia.Dia,
                MaxComidas = dia.MaxComidas,
                Observaciones = dia.Observaciones?.Trim(),
                Comidas = dia.Comidas.Select(comida => new MenuPlantillaComida
                {
                    TipoComida = comida.TipoComida.Trim(),
                    Orden = comida.Orden,
                    RecetaId = comida.RecetaId,
                    Porciones = comida.Porciones,
                    Hora = comida.Hora,
                    Observaciones = comida.Observaciones?.Trim()
                }).ToList()
            }).ToList()
        };

        var created = await _repository.CreatePlantillaAsync(plantilla);
        return MapPlantilla(created);
    }

    public async Task<MenuPlantillaResult> UpdatePlantillaAsync(int id, MenuPlantillaInput input)
    {
        _accessControl.EnsureStaffWeb();
        var plantilla = await _repository.GetPlantillaByIdAsync(id)
                        ?? throw new KeyNotFoundException("La plantilla no existe.");
        await EnsureCanEditPlantillaAsync(plantilla);
        ValidatePlantillaInput(input);
        await ValidateTemplatesRecipesAsync(input);

        plantilla.Nombre = input.Nombre.Trim();
        plantilla.Descripcion = input.Descripcion?.Trim();
        plantilla.Dias = input.Dias.Select(dia => new MenuPlantillaDia
        {
            Dia = dia.Dia,
            MaxComidas = dia.MaxComidas,
            Observaciones = dia.Observaciones?.Trim(),
            Comidas = dia.Comidas.Select(comida => new MenuPlantillaComida
            {
                TipoComida = comida.TipoComida.Trim(),
                Orden = comida.Orden,
                RecetaId = comida.RecetaId,
                Porciones = comida.Porciones,
                Hora = comida.Hora,
                Observaciones = comida.Observaciones?.Trim()
            }).ToList()
        }).ToList();

        var updated = await _repository.UpdatePlantillaAsync(plantilla);
        return MapPlantilla(updated);
    }

    public async Task<bool> DeletePlantillaAsync(int id)
    {
        _accessControl.EnsureStaffWeb();
        var plantilla = await _repository.GetPlantillaByIdAsync(id);
        if (plantilla is null)
        {
            return false;
        }

        await EnsureCanEditPlantillaAsync(plantilla);
        return await _repository.DeletePlantillaAsync(id);
    }

    public async Task<MenuPlantillaResult> CopyPlantillaAsync(int id, int? workspaceId)
    {
        _accessControl.EnsureStaffWeb();
        var current = _accessControl.GetCurrentUser();
        var original = await _repository.GetPlantillaByIdAsync(id)
                       ?? throw new KeyNotFoundException("La plantilla no existe.");
        await EnsureCanViewPlantillaAsync(original);

        var copy = new MenuPlantilla
        {
            WorkspaceId = null,
            Nombre = original.Nombre,
            Descripcion = original.Descripcion,
            PlantillaOrigenId = original.Id,
            CreatedByUserId = current.UserId,
            CreatedBy = current.UserId.ToString(),
            Dias = original.Dias.Select(dia => new MenuPlantillaDia
            {
                Dia = dia.Dia,
                MaxComidas = dia.MaxComidas,
                Observaciones = dia.Observaciones,
                Comidas = dia.Comidas.Select(comida => new MenuPlantillaComida
                {
                    TipoComida = comida.TipoComida,
                    Orden = comida.Orden,
                    RecetaId = comida.RecetaId,
                    Porciones = comida.Porciones,
                    Hora = comida.Hora,
                    Observaciones = comida.Observaciones
                }).ToList()
            }).ToList()
        };

        var created = await _repository.CreatePlantillaAsync(copy);
        return MapPlantilla(created);
    }

    public async Task<MenuPlantillaResult> CreatePlantillaFromMenuAsync(
        int menuId,
        CreatePlantillaFromMenuInput input)
    {
        _accessControl.EnsureStaffWeb();
        var current = _accessControl.GetCurrentUser();
        var menu = await _repository.GetMenuByIdAsync(menuId)
                   ?? throw new KeyNotFoundException("El menú no existe.");
        await EnsureCanViewMenuAsync(menu);

        var plantilla = new MenuPlantilla
        {
            WorkspaceId = null,
            Nombre = input.Nombre.Trim(),
            Descripcion = input.Descripcion?.Trim(),
            CreatedByUserId = current.UserId,
            CreatedBy = current.UserId.ToString(),
            Dias = menu.Dias
                .Where(dia => dia.IsActive)
                .OrderBy(dia => dia.Fecha)
                .Select(dia => new MenuPlantillaDia
                {
                    Dia = dia.Fecha.Day,
                    MaxComidas = dia.MaxComidas,
                    Observaciones = dia.Observaciones,
                    Comidas = dia.Comidas
                        .Where(comida => comida.IsActive)
                        .OrderBy(comida => comida.Orden)
                        .Select(comida => new MenuPlantillaComida
                        {
                            TipoComida = comida.TipoComida,
                            Orden = comida.Orden,
                            RecetaId = comida.RecetaId,
                            Porciones = comida.Porciones,
                            Hora = comida.Hora,
                            Observaciones = comida.Observaciones
                        }).ToList()
                }).ToList()
        };

        var created = await _repository.CreatePlantillaAsync(plantilla);
        return MapPlantilla(created);
    }

    public async Task<MenuMensualResult> CreateMenuAsync(MenuMensualInput input)
    {
        _accessControl.EnsureStaffWeb();
        ValidateMenuInput(input);
        await EnsureMenuScopeAsync(input.PacienteUserId, input.WorkspaceId);

        if (await _repository.GetMenuByPatientMonthAsync(
                input.PacienteUserId,
                input.WorkspaceId,
                input.Anio,
                input.Mes) is not null)
        {
            throw new InvalidOperationException(
                "El paciente ya tiene un menú para ese mes en este workspace.");
        }

        var current = _accessControl.GetCurrentUser();
        var menu = new MenuMensual
        {
            PacienteUserId = input.PacienteUserId,
            WorkspaceId = input.WorkspaceId,
            Anio = input.Anio,
            Mes = input.Mes,
            Estado = "Borrador",
            Observaciones = input.Observaciones?.Trim(),
            CreadoPorUserId = current.UserId,
            CreatedBy = current.UserId.ToString(),
            Dias = BuildEmptyDays(input.Anio, input.Mes, input.MaxComidasDefault)
        };

        var created = await _repository.CreateMenuAsync(menu);
        return MapMenu(created);
    }

    public async Task<MenuMensualResult> CreateMenuFromPlantillaAsync(
        int plantillaId,
        MenuMensualInput input)
    {
        _accessControl.EnsureStaffWeb();
        ValidateMenuInput(input);
        await EnsureMenuScopeAsync(input.PacienteUserId, input.WorkspaceId);

        if (await _repository.GetMenuByPatientMonthAsync(
                input.PacienteUserId,
                input.WorkspaceId,
                input.Anio,
                input.Mes) is not null)
        {
            throw new InvalidOperationException(
                "El paciente ya tiene un menú para ese mes en este workspace.");
        }

        var plantilla = await _repository.GetPlantillaByIdAsync(plantillaId)
                        ?? throw new KeyNotFoundException("La plantilla no existe.");
        await EnsureCanViewPlantillaAsync(plantilla);

        var current = _accessControl.GetCurrentUser();
        var menu = new MenuMensual
        {
            PacienteUserId = input.PacienteUserId,
            WorkspaceId = input.WorkspaceId,
            Anio = input.Anio,
            Mes = input.Mes,
            Estado = "Borrador",
            PlantillaOrigenId = plantilla.Id,
            Observaciones = input.Observaciones?.Trim(),
            CreadoPorUserId = current.UserId,
            CreatedBy = current.UserId.ToString(),
            Dias = new List<MenuDia>()
        };

        var recipesCache = new Dictionary<int, Receta>();
        var daysInMonth = DateTime.DaysInMonth(input.Anio, input.Mes);

        for (var day = 1; day <= daysInMonth; day++)
        {
            var templateDay = plantilla.Dias.FirstOrDefault(item => item.Dia == day);
            var menuDay = new MenuDia
            {
                Fecha = new DateOnly(input.Anio, input.Mes, day),
                MaxComidas = templateDay?.MaxComidas ?? input.MaxComidasDefault,
                Observaciones = templateDay?.Observaciones
            };

            if (templateDay is not null)
            {
                foreach (var templateMeal in templateDay.Comidas.Where(item => item.IsActive))
                {
                    var recipe = await GetRecipeForMealAsync(templateMeal.RecetaId, recipesCache);
                    menuDay.Comidas.Add(new MenuComida
                    {
                        TipoComida = templateMeal.TipoComida,
                        Orden = templateMeal.Orden,
                        RecetaId = recipe.Id,
                        RecetaSnapshotJson = BuildRecipeSnapshot(recipe, templateMeal.Porciones),
                        Porciones = templateMeal.Porciones,
                        Hora = templateMeal.Hora,
                        Observaciones = templateMeal.Observaciones
                    });
                }
            }

            menu.Dias.Add(menuDay);
        }

        var created = await _repository.CreateMenuAsync(menu);
        return MapMenu(created);
    }

    public async Task<MenuMensualResult?> GetMenuAsync(int id)
    {
        _accessControl.EnsureStaffWeb();
        var menu = await _repository.GetMenuByIdAsync(id);
        if (menu is null)
        {
            return null;
        }

        await EnsureCanViewMenuAsync(menu);
        return MapMenu(menu);
    }

    public async Task<ICollection<MenuMensualResult>> GetMenusByPatientAsync(
        int patientId,
        int? anio,
        int? mes,
        int? workspaceId)
    {
        _accessControl.EnsureStaffWeb();
        var current = _accessControl.GetCurrentUser();
        await _accessControl.EnsureCanViewPatientAsync(patientId);

        if (workspaceId.HasValue)
        {
            await EnsureWorkspaceAccessAsync(workspaceId.Value);
        }

        var menus = await _repository.GetMenusByPatientAsync(
            patientId,
            anio,
            mes,
            workspaceId);

        if (!current.IsAdmin)
        {
            var workspaceIds = await _workspaceMemberRepository.GetActiveWorkspaceIdsAsync(
                current.UserId,
                AppRoles.Nutricionista);
            menus = menus.Where(menu => workspaceIds.Contains(menu.WorkspaceId)).ToList();
        }

        return menus.Select(MapMenu).ToList();
    }

    public async Task<MenuMensualResult> UpdateMenuAsync(int id, MenuMensualUpdateInput input)
    {
        _accessControl.EnsureStaffWeb();
        var menu = await _repository.GetMenuByIdAsync(id)
                   ?? throw new KeyNotFoundException("El menú no existe.");
        await EnsureCanEditMenuAsync(menu);

        menu.Observaciones = input.Observaciones?.Trim();
        var updated = await _repository.UpdateMenuAsync(menu);
        return MapMenu(updated);
    }

    public async Task<MenuMensualResult> UpdateMenuDayAsync(
        int menuId,
        DateOnly fecha,
        MenuDiaUpdateInput input)
    {
        _accessControl.EnsureStaffWeb();
        var menu = await _repository.GetMenuByIdAsync(menuId)
                   ?? throw new KeyNotFoundException("El menú no existe.");
        await EnsureCanEditMenuAsync(menu);

        var day = menu.Dias.FirstOrDefault(item => item.IsActive && item.Fecha == fecha)
                  ?? throw new KeyNotFoundException("El día no existe en el menú.");

        if (input.MaxComidas is < 1 or > 8)
        {
            throw new InvalidOperationException("La cantidad máxima de comidas debe estar entre 1 y 8.");
        }

        var activeMeals = day.Comidas.Count(item => item.IsActive);
        if (input.MaxComidas < activeMeals)
        {
            throw new InvalidOperationException(
                $"No puedes dejar {input.MaxComidas} comidas cuando ya hay {activeMeals} asignadas.");
        }

        day.MaxComidas = input.MaxComidas;
        day.Observaciones = input.Observaciones?.Trim();
        var updated = await _repository.UpdateMenuAsync(menu);
        return MapMenu(updated);
    }

    public async Task<MenuMensualResult> AddMealAsync(
        int menuId,
        DateOnly fecha,
        MenuComidaInput input)
    {
        _accessControl.EnsureStaffWeb();
        var menu = await _repository.GetMenuByIdAsync(menuId)
                   ?? throw new KeyNotFoundException("El menú no existe.");
        await EnsureCanEditMenuAsync(menu);

        var day = menu.Dias.FirstOrDefault(item => item.IsActive && item.Fecha == fecha)
                  ?? throw new KeyNotFoundException("El día no existe en el menú.");

        ValidateMealInput(input);
        var activeMeals = day.Comidas.Where(item => item.IsActive).ToList();
        if (activeMeals.Count >= day.MaxComidas)
        {
            throw new InvalidOperationException(
                $"Este día ya tiene el máximo de {day.MaxComidas} comidas.");
        }

        if (activeMeals.Any(item => item.Orden == input.Orden))
        {
            throw new InvalidOperationException("Ya existe una comida con ese orden en el día.");
        }

        var current = _accessControl.GetCurrentUser();
        var recipe = await GetRecipeForMealAsync(input.RecetaId, new Dictionary<int, Receta>());
        day.Comidas.Add(new MenuComida
        {
            TipoComida = input.TipoComida.Trim(),
            Orden = input.Orden,
            RecetaId = recipe.Id,
            RecetaSnapshotJson = BuildRecipeSnapshot(recipe, input.Porciones),
            Porciones = input.Porciones,
            Hora = input.Hora,
            Observaciones = input.Observaciones?.Trim(),
            CreatedBy = current.UserId.ToString()
        });

        var updated = await _repository.UpdateMenuAsync(menu);
        return MapMenu(updated);
    }

    public async Task<MenuMensualResult> UpdateMealAsync(
        int menuId,
        int comidaId,
        MenuComidaInput input)
    {
        _accessControl.EnsureStaffWeb();
        var menu = await _repository.GetMenuByIdAsync(menuId)
                   ?? throw new KeyNotFoundException("El menú no existe.");
        await EnsureCanEditMenuAsync(menu);

        var meal = menu.Dias
            .SelectMany(day => day.Comidas)
            .FirstOrDefault(item => item.Id == comidaId && item.IsActive)
            ?? throw new KeyNotFoundException("La comida no existe en el menú.");

        ValidateMealInput(input);

        var day = meal.MenuDia;
        if (day.Comidas.Any(item => item.Id != meal.Id && item.IsActive && item.Orden == input.Orden))
        {
            throw new InvalidOperationException("Ya existe otra comida con ese orden en el día.");
        }

        var recipe = await GetRecipeForMealAsync(input.RecetaId, new Dictionary<int, Receta>());
        meal.TipoComida = input.TipoComida.Trim();
        meal.Orden = input.Orden;
        meal.RecetaId = recipe.Id;
        meal.RecetaSnapshotJson = BuildRecipeSnapshot(recipe, input.Porciones);
        meal.Porciones = input.Porciones;
        meal.Hora = input.Hora;
        meal.Observaciones = input.Observaciones?.Trim();

        var updated = await _repository.UpdateMenuAsync(menu);
        return MapMenu(updated);
    }

    public async Task<MenuMensualResult> DeleteMealAsync(int menuId, int comidaId)
    {
        _accessControl.EnsureStaffWeb();
        var menu = await _repository.GetMenuByIdAsync(menuId)
                   ?? throw new KeyNotFoundException("El menú no existe.");
        await EnsureCanEditMenuAsync(menu);

        var meal = menu.Dias
            .SelectMany(day => day.Comidas)
            .FirstOrDefault(item => item.Id == comidaId && item.IsActive)
            ?? throw new KeyNotFoundException("La comida no existe en el menú.");

        var current = _accessControl.GetCurrentUser();
        meal.IsActive = false;
        meal.DeletedOn = DateTime.UtcNow;
        meal.DeletedBy = current.UserId.ToString();

        var updated = await _repository.UpdateMenuAsync(menu);
        return MapMenu(updated);
    }

    public async Task<bool> DeleteMenuAsync(int id)
    {
        _accessControl.EnsureStaffWeb();
        var menu = await _repository.GetMenuByIdAsync(id);
        if (menu is null)
        {
            return false;
        }

        await EnsureCanEditMenuAsync(menu);
        var current = _accessControl.GetCurrentUser();
        return await _repository.DeleteMenuAsync(id, current.UserId.ToString());
    }

    public async Task<MenuMensualResult> PublishMenuAsync(int id)
    {
        _accessControl.EnsureStaffWeb();
        var menu = await _repository.GetMenuByIdAsync(id)
                   ?? throw new KeyNotFoundException("El menú no existe.");
        await EnsureCanEditMenuAsync(menu);

        var current = _accessControl.GetCurrentUser();
        menu.Estado = "Publicado";
        menu.PublicadoPorUserId = current.UserId;
        menu.PublicadoEn = DateTime.UtcNow;

        var updated = await _repository.UpdateMenuAsync(menu);
        return MapMenu(updated);
    }

    public async Task<MenuMensualResult?> GetMyMenuAsync(
        int anio,
        int mes,
        int? workspaceId)
    {
        var current = _accessControl.GetCurrentUser();
        EnsurePatientAccess(current);

        var menu = await _repository.GetPublishedMenuAsync(
            current.UserId,
            anio,
            mes,
            workspaceId);

        return menu is null ? null : MapMenu(menu);
    }

    public async Task<ICollection<MenuMensualResult>> GetMyMenusAsync(int anio, int mes)
    {
        var current = _accessControl.GetCurrentUser();
        EnsurePatientAccess(current);

        var menus = await _repository.GetPublishedMenusAsync(current.UserId, anio, mes);
        return menus.Select(MapMenu).ToList();
    }

    public async Task<MenuMensualResult?> GetMyMenuDayAsync(
        DateOnly fecha,
        int? workspaceId)
    {
        var current = _accessControl.GetCurrentUser();
        EnsurePatientAccess(current);

        var menu = await _repository.GetPublishedMenuAsync(
            current.UserId,
            fecha.Year,
            fecha.Month,
            workspaceId);

        if (menu is null)
        {
            return null;
        }

        var result = MapMenu(menu);
        result.Dias = result.Dias.Where(day => day.Fecha == fecha).ToList();
        return result;
    }

    private async Task EnsureWorkspaceAccessAsync(int workspaceId)
    {
        var current = _accessControl.GetCurrentUser();
        if (current.IsAdmin)
        {
            var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
            if (workspace is null)
            {
                throw new KeyNotFoundException("El workspace no existe.");
            }

            return;
        }

        await _accessControl.EnsureCanViewWorkspaceAsync(workspaceId);
    }

    private async Task EnsureCanViewRecetaAsync(Receta receta)
    {
        var current = _accessControl.GetCurrentUser();
        if (current.IsAdmin || receta.WorkspaceId is null)
        {
            return;
        }

        await _accessControl.EnsureCanViewWorkspaceAsync(receta.WorkspaceId.Value);
    }

    private Task EnsureCanEditRecetaAsync(Receta receta)
    {
        // Las recetas son un catálogo compartido: cualquier miembro del staff
        // puede editarlas. Se conserva la validación por workspace para datos
        // heredados que todavía tengan un WorkspaceId asignado.
        return EnsureCanViewRecetaAsync(receta);
    }

    private async Task EnsureCanViewPlantillaAsync(MenuPlantilla plantilla)
    {
        var current = _accessControl.GetCurrentUser();
        if (current.IsAdmin || plantilla.WorkspaceId is null)
        {
            return;
        }

        await _accessControl.EnsureCanViewWorkspaceAsync(plantilla.WorkspaceId.Value);
    }

    private Task EnsureCanEditPlantillaAsync(MenuPlantilla plantilla)
    {
        return EnsureCanViewPlantillaAsync(plantilla);
    }

    private async Task EnsureMenuScopeAsync(int patientId, int workspaceId)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        if (workspace is null)
        {
            throw new KeyNotFoundException("El workspace no existe.");
        }

        await EnsureWorkspaceAccessAsync(workspaceId);
        await _accessControl.EnsureCanViewPatientAsync(patientId);

        var member = await _workspaceMemberRepository
            .GetActiveByUserAndWorkspaceAsync(patientId, workspaceId);

        if (member is null ||
            !string.Equals(member.MemberRole, AppRoles.Paciente, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "El paciente no pertenece al workspace seleccionado con el rol Paciente.");
        }
    }

    private async Task EnsureCanViewMenuAsync(MenuMensual menu)
    {
        var current = _accessControl.GetCurrentUser();
        if (current.IsAdmin)
        {
            return;
        }

        await _accessControl.EnsureCanViewWorkspaceAsync(menu.WorkspaceId);
    }

    private Task EnsureCanEditMenuAsync(MenuMensual menu)
    {
        return EnsureCanViewMenuAsync(menu);
    }

    private static void EnsurePatientAccess(CurrentUser current)
    {
        if (!current.IsPaciente || !current.IsMobile)
        {
            throw new ForbiddenAccessException(
                "Esta información solo está disponible para el paciente desde la app móvil.");
        }
    }

    private static void ValidateRecetaInput(RecetaInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre))
        {
            throw new InvalidOperationException("El nombre de la receta es obligatorio.");
        }

        if (input.PorcionesBase < 1)
        {
            throw new InvalidOperationException("Las porciones base deben ser mayores que cero.");
        }

        if (input.Ingredientes.Count == 0)
        {
            throw new InvalidOperationException("La receta debe tener al menos un producto.");
        }

        var productIds = new HashSet<long>();
        foreach (var ingredient in input.Ingredientes)
        {
            if (ingredient.ProductoId <= 0)
            {
                throw new InvalidOperationException("Todos los ingredientes deben tener un producto válido.");
            }

            if (!productIds.Add(ingredient.ProductoId))
            {
                throw new InvalidOperationException("No se puede repetir el mismo producto en una receta.");
            }

            if (ingredient.Cantidad <= 0)
            {
                throw new InvalidOperationException("La cantidad de cada ingrediente debe ser mayor que cero.");
            }

            if (string.IsNullOrWhiteSpace(ingredient.Unidad))
            {
                throw new InvalidOperationException("La unidad de cada ingrediente es obligatoria.");
            }
        }
    }

    private static void ValidatePlantillaInput(MenuPlantillaInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre))
        {
            throw new InvalidOperationException("El nombre de la plantilla es obligatorio.");
        }

        if (input.Dias.Count == 0)
        {
            throw new InvalidOperationException("La plantilla debe tener al menos un día.");
        }

        var dayNumbers = new HashSet<int>();
        foreach (var day in input.Dias)
        {
            if (day.Dia is < 1 or > 31)
            {
                throw new InvalidOperationException("Los días de la plantilla deben estar entre 1 y 31.");
            }

            if (!dayNumbers.Add(day.Dia))
            {
                throw new InvalidOperationException("No se puede repetir un día en la plantilla.");
            }

            if (day.MaxComidas is < 1 or > 8)
            {
                throw new InvalidOperationException("La cantidad máxima de comidas debe estar entre 1 y 8.");
            }

            if (day.Comidas.Count > day.MaxComidas)
            {
                throw new InvalidOperationException(
                    $"El día {day.Dia} tiene más comidas que su máximo permitido.");
            }

            var orders = new HashSet<int>();
            foreach (var meal in day.Comidas)
            {
                if (string.IsNullOrWhiteSpace(meal.TipoComida))
                {
                    throw new InvalidOperationException("El tipo de comida es obligatorio.");
                }

                if (!orders.Add(meal.Orden))
                {
                    throw new InvalidOperationException(
                        $"El día {day.Dia} tiene dos comidas con el mismo orden.");
                }

                if (meal.RecetaId is null)
                {
                    throw new InvalidOperationException("Cada comida debe tener una receta.");
                }

                if (meal.Porciones <= 0)
                {
                    throw new InvalidOperationException("Las porciones deben ser mayores que cero.");
                }
            }
        }
    }

    private async Task ValidateTemplatesRecipesAsync(MenuPlantillaInput input)
    {
        var recipeIds = input.Dias
            .SelectMany(day => day.Comidas)
            .Select(meal => meal.RecetaId!.Value)
            .Distinct();

        foreach (var recipeId in recipeIds)
        {
            var recipe = await _repository.GetRecetaByIdAsync(recipeId)
                         ?? throw new InvalidOperationException($"La receta {recipeId} no existe.");
            await EnsureCanViewRecetaAsync(recipe);
        }
    }

    private static void ValidateMenuInput(MenuMensualInput input)
    {
        if (input.PacienteUserId <= 0)
        {
            throw new InvalidOperationException("El paciente es obligatorio.");
        }

        if (input.WorkspaceId <= 0)
        {
            throw new InvalidOperationException("El workspace es obligatorio.");
        }

        if (input.Anio is < 2000 or > 2100)
        {
            throw new InvalidOperationException("El año del menú no es válido.");
        }

        if (input.Mes is < 1 or > 12)
        {
            throw new InvalidOperationException("El mes del menú debe estar entre 1 y 12.");
        }

        if (input.MaxComidasDefault is < 1 or > 8)
        {
            throw new InvalidOperationException("La cantidad máxima de comidas debe estar entre 1 y 8.");
        }
    }

    private static void ValidateMealInput(MenuComidaInput input)
    {
        if (string.IsNullOrWhiteSpace(input.TipoComida))
        {
            throw new InvalidOperationException("El tipo de comida es obligatorio.");
        }

        if (input.RecetaId <= 0)
        {
            throw new InvalidOperationException("La receta es obligatoria.");
        }

        if (input.Porciones <= 0)
        {
            throw new InvalidOperationException("Las porciones deben ser mayores que cero.");
        }

        if (input.Orden <= 0)
        {
            throw new InvalidOperationException("El orden de la comida debe ser mayor que cero.");
        }
    }

    private static List<MenuDia> BuildEmptyDays(int anio, int mes, int maxComidas)
    {
        var daysInMonth = DateTime.DaysInMonth(anio, mes);
        return Enumerable.Range(1, daysInMonth)
            .Select(day => new MenuDia
            {
                Fecha = new DateOnly(anio, mes, day),
                MaxComidas = maxComidas
            })
            .ToList();
    }

    private async Task<Receta> GetRecipeForMealAsync(
        int? recipeId,
        Dictionary<int, Receta> cache)
    {
        if (!recipeId.HasValue)
        {
            throw new InvalidOperationException("La comida debe tener una receta.");
        }

        if (cache.TryGetValue(recipeId.Value, out var cached))
        {
            return cached;
        }

        var recipe = await _repository.GetRecetaByIdAsync(recipeId.Value)
                     ?? throw new InvalidOperationException("La receta no existe.");
        await EnsureCanViewRecetaAsync(recipe);
        cache[recipeId.Value] = recipe;
        return recipe;
    }

    private static string BuildRecipeSnapshot(Receta receta, decimal porciones)
    {
        var snapshot = new
        {
            recetaId = receta.Id,
            nombre = receta.Nombre,
            descripcion = receta.Descripcion,
            instrucciones = receta.Instrucciones,
            porcionesBase = receta.PorcionesBase,
            porciones,
            ingredientes = receta.Ingredientes
                .Where(item => item.IsActive)
                .Select(item => new
                {
                    productoId = item.ProductoId,
                    nombre = item.Producto?.Nombre,
                    marca = item.Producto?.Marca,
                    cantidad = item.Cantidad,
                    unidad = item.Unidad,
                    precio = item.Producto?.Precio,
                    imagenUrlOrigen = item.Producto?.ImagenUrl
                })
        };

        return JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static RecetaResult MapReceta(Receta receta)
    {
        return new RecetaResult
        {
            Id = receta.Id,
            WorkspaceId = receta.WorkspaceId,
            Nombre = receta.Nombre,
            Descripcion = receta.Descripcion,
            Instrucciones = receta.Instrucciones,
            PorcionesBase = receta.PorcionesBase,
            RecetaOrigenId = receta.RecetaOrigenId,
            CreatedByUserId = receta.CreatedByUserId,
            IsActive = receta.IsActive,
            Ingredientes = receta.Ingredientes
                .Where(item => item.IsActive)
                .Select(item => new RecetaIngredienteResult
                {
                    Id = item.Id,
                    ProductoId = item.ProductoId,
                    ProductoNombre = item.Producto?.Nombre ?? string.Empty,
                    ProductoMarca = item.Producto?.Marca,
                    ProductoImagenUrl = item.Producto?.ImagenUrl,
                    ProductoPrecio = item.Producto?.Precio,
                    Cantidad = item.Cantidad,
                    Unidad = item.Unidad,
                    Notas = item.Notas
                }).ToList()
        };
    }

    private static MenuPlantillaResult MapPlantilla(MenuPlantilla plantilla)
    {
        return new MenuPlantillaResult
        {
            Id = plantilla.Id,
            WorkspaceId = plantilla.WorkspaceId,
            Nombre = plantilla.Nombre,
            Descripcion = plantilla.Descripcion,
            PlantillaOrigenId = plantilla.PlantillaOrigenId,
            CreatedByUserId = plantilla.CreatedByUserId,
            IsActive = plantilla.IsActive,
            Dias = plantilla.Dias
                .Where(day => day.IsActive)
                .OrderBy(day => day.Dia)
                .Select(day => new MenuPlantillaDiaResult
                {
                    Id = day.Id,
                    Dia = day.Dia,
                    MaxComidas = day.MaxComidas,
                    Observaciones = day.Observaciones,
                    Comidas = day.Comidas
                        .Where(meal => meal.IsActive)
                        .OrderBy(meal => meal.Orden)
                        .Select(meal => new MenuPlantillaComidaResult
                        {
                            Id = meal.Id,
                            TipoComida = meal.TipoComida,
                            Orden = meal.Orden,
                            RecetaId = meal.RecetaId,
                            RecetaNombre = meal.Receta?.Nombre,
                            Porciones = meal.Porciones,
                            Hora = meal.Hora,
                            Observaciones = meal.Observaciones
                        }).ToList()
                }).ToList()
        };
    }

    private static JsonElement ParseJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(json) ? "{}" : json);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            using var document = JsonDocument.Parse("{}");
            return document.RootElement.Clone();
        }
    }

    private static MenuMensualResult MapMenu(MenuMensual menu)
    {
        return new MenuMensualResult
        {
            Id = menu.Id,
            PacienteUserId = menu.PacienteUserId,
            PacienteNombre = menu.Paciente is null
                ? string.Empty
                : $"{menu.Paciente.Nombres} {menu.Paciente.Apellidos}".Trim(),
            WorkspaceId = menu.WorkspaceId,
            WorkspaceName = menu.Workspace?.Name ?? string.Empty,
            Anio = menu.Anio,
            Mes = menu.Mes,
            Estado = menu.Estado,
            PlantillaOrigenId = menu.PlantillaOrigenId,
            Observaciones = menu.Observaciones,
            CreadoPorUserId = menu.CreadoPorUserId,
            PublicadoPorUserId = menu.PublicadoPorUserId,
            PublicadoEn = menu.PublicadoEn,
            IsActive = menu.IsActive,
            Dias = menu.Dias
                .Where(day => day.IsActive)
                .OrderBy(day => day.Fecha)
                .Select(day => new MenuDiaResult
                {
                    Id = day.Id,
                    Fecha = day.Fecha,
                    MaxComidas = day.MaxComidas,
                    Observaciones = day.Observaciones,
                    Comidas = day.Comidas
                        .Where(meal => meal.IsActive)
                        .OrderBy(meal => meal.Orden)
                        .Select(meal => new MenuComidaResult
                        {
                            Id = meal.Id,
                            TipoComida = meal.TipoComida,
                            Orden = meal.Orden,
                            RecetaId = meal.RecetaId,
                            RecetaNombre = meal.Receta?.Nombre,
                            RecetaSnapshotJson = ParseJson(meal.RecetaSnapshotJson),
                            Porciones = meal.Porciones,
                            Hora = meal.Hora,
                            Observaciones = meal.Observaciones
                        }).ToList()
                }).ToList()
        };
    }
}
