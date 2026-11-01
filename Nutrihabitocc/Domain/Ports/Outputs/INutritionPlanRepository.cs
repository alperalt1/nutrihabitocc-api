using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface INutritionPlanRepository
{
    Task<ICollection<Receta>> GetRecetasAsync(string? q);
    Task<Receta?> GetRecetaByIdAsync(int id);
    Task<Receta> CreateRecetaAsync(Receta receta);
    Task<Receta> UpdateRecetaAsync(Receta receta);
    Task<bool> DeleteRecetaAsync(int id);

    Task<ICollection<MenuPlantilla>> GetPlantillasAsync(string? q);
    Task<MenuPlantilla?> GetPlantillaByIdAsync(int id);
    Task<MenuPlantilla> CreatePlantillaAsync(MenuPlantilla plantilla);
    Task<MenuPlantilla> UpdatePlantillaAsync(MenuPlantilla plantilla);
    Task<bool> DeletePlantillaAsync(int id);

    Task<MenuMensual?> GetMenuByIdAsync(int id);
    Task<MenuMensual?> GetMenuByPatientMonthAsync(int patientId, int workspaceId, int anio, int mes);
    Task<MenuMensual?> GetPublishedMenuAsync(
        int patientId,
        int anio,
        int mes,
        int? workspaceId);
    Task<ICollection<MenuMensual>> GetPublishedMenusAsync(int patientId, int anio, int mes);
    Task<ICollection<MenuMensual>> GetMenusByPatientAsync(
        int patientId,
        int? anio,
        int? mes,
        int? workspaceId);
    Task<MenuMensual> CreateMenuAsync(MenuMensual menu);
    Task<MenuMensual> UpdateMenuAsync(MenuMensual menu);
    Task<bool> DeleteMenuAsync(int id, string? deletedBy);
    Task<MenuDia?> GetMenuDiaAsync(int menuId, DateOnly fecha);
    Task<MenuComida?> GetMenuComidaAsync(int menuId, int comidaId);
}
