using Nutrihabitocc.Domain.Dtos;

namespace Nutrihabitocc.Domain.Ports.Inputs;

public interface INutritionPlanService
{
    Task<ICollection<RecetaResult>> GetRecetasAsync(int? workspaceId, string? q);
    Task<RecetaResult?> GetRecetaByIdAsync(int id);
    Task<RecetaResult> CreateRecetaAsync(RecetaInput input);
    Task<RecetaResult> UpdateRecetaAsync(int id, RecetaInput input);
    Task<bool> DeleteRecetaAsync(int id);
    Task<RecetaResult> CopyRecetaAsync(int id, int? workspaceId);

    Task<ICollection<MenuPlantillaResult>> GetPlantillasAsync(int? workspaceId, string? q);
    Task<MenuPlantillaResult?> GetPlantillaByIdAsync(int id);
    Task<MenuPlantillaResult> CreatePlantillaAsync(MenuPlantillaInput input);
    Task<MenuPlantillaResult> UpdatePlantillaAsync(int id, MenuPlantillaInput input);
    Task<bool> DeletePlantillaAsync(int id);
    Task<MenuPlantillaResult> CopyPlantillaAsync(int id, int? workspaceId);
    Task<MenuPlantillaResult> CreatePlantillaFromMenuAsync(int menuId, CreatePlantillaFromMenuInput input);

    Task<MenuMensualResult> CreateMenuAsync(MenuMensualInput input);
    Task<MenuMensualResult> CreateMenuFromPlantillaAsync(int plantillaId, MenuMensualInput input);
    Task<MenuMensualResult?> GetMenuAsync(int id);
    Task<ICollection<MenuMensualResult>> GetMenusByPatientAsync(
        int patientId,
        int? anio,
        int? mes,
        int? workspaceId);
    Task<MenuMensualResult> UpdateMenuAsync(int id, MenuMensualUpdateInput input);
    Task<MenuMensualResult> UpdateMenuDayAsync(int menuId, DateOnly fecha, MenuDiaUpdateInput input);
    Task<MenuMensualResult> AddMealAsync(int menuId, DateOnly fecha, MenuComidaInput input);
    Task<MenuMensualResult> UpdateMealAsync(int menuId, int comidaId, MenuComidaInput input);
    Task<MenuMensualResult> DeleteMealAsync(int menuId, int comidaId);
    Task<bool> DeleteMenuAsync(int id);
    Task<MenuMensualResult> PublishMenuAsync(int id);
    Task<MenuMensualResult?> GetMyMenuAsync(int anio, int mes, int? workspaceId);
    Task<ICollection<MenuMensualResult>> GetMyMenusAsync(int anio, int mes);
    Task<MenuMensualResult?> GetMyMenuDayAsync(DateOnly fecha, int? workspaceId);
}
