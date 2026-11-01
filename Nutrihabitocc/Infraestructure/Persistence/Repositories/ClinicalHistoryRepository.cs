using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Persistence.Repositories;

public class ClinicalHistoryRepository: IClinicalHistoryRepository
{
    private readonly AppDbContext _context;

    public ClinicalHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<ClinicalHistory>> GetAllAsync(
        int? workspaceId,
        int? userId,
        int? nutritionistUserId)
    {
        var query = BuildReadQuery().Where(history => history.IsActive);

        if (workspaceId.HasValue)
        {
            query = query.Where(history => history.WorkspaceId == workspaceId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(history => history.UserId == userId.Value);
        }

        if (nutritionistUserId.HasValue)
        {
            query = query.Where(history =>
                history.WorkspaceId.HasValue &&
                _context.WorkspaceMembers.Any(member =>
                    member.WorkspaceId == history.WorkspaceId &&
                    member.UserId == nutritionistUserId.Value &&
                    member.IsActive &&
                    member.MemberRole == AppRoles.Nutricionista));
        }

        return await query
            .OrderByDescending(history => history.CreatedOn)
            .ToListAsync();
    }

    public async Task<ICollection<ClinicalHistory>> GetByWorkspaceAsync(int workspaceId)
    {
        return await BuildReadQuery()
            .Where(history => history.WorkspaceId == workspaceId && history.IsActive)
            .OrderByDescending(history => history.CreatedOn)
            .ToListAsync();
    }

    public async Task<ICollection<ClinicalHistory>> GetByPatientAsync(int userId)
    {
        return await BuildReadQuery()
            .Where(history => history.UserId == userId && history.IsActive)
            .OrderByDescending(history => history.CreatedOn)
            .ToListAsync();
    }

    public async Task<ClinicalHistory?> GetByIdAsync(int id)
    {
        return await BuildReadQuery()
            .FirstOrDefaultAsync(history => history.Id == id && history.IsActive);
    }

    public async Task<ClinicalHistory> CreateAsync(ClinicalHistory clinicalHistory)
    {
        _context.ClinicalHistories.Add(clinicalHistory);
        await _context.SaveChangesAsync();
        return clinicalHistory;
    }

    public async Task<bool> UpdateContentAsync(int id, string contentJson)
    {
        var clinicalHistory = await _context.ClinicalHistories
            .FirstOrDefaultAsync(history => history.Id == id && history.IsActive);

        if (clinicalHistory is null)
        {
            return false;
        }

        if (clinicalHistory.ContentJson != contentJson)
        {
            clinicalHistory.ContentJson = contentJson;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> AssignWorkspaceAsync(int id, int? workspaceId)
    {
        var clinicalHistory = await _context.ClinicalHistories
            .FirstOrDefaultAsync(history => history.Id == id && history.IsActive);

        if (clinicalHistory is null)
        {
            return false;
        }

        if (clinicalHistory.WorkspaceId != workspaceId)
        {
            clinicalHistory.WorkspaceId = workspaceId;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var clinicalHistory = await _context.ClinicalHistories
            .FirstOrDefaultAsync(history => history.Id == id && history.IsActive);

        if (clinicalHistory is null)
        {
            return false;
        }

        _context.ClinicalHistories.Remove(clinicalHistory);
        return await _context.SaveChangesAsync() > 0;
    }

    private IQueryable<ClinicalHistory> BuildReadQuery()
    {
        return _context.ClinicalHistories
            .AsNoTracking()
            .Include(history => history.User)
            .Include(history => history.Workspace)
            .Include(history => history.PlantillaFormulario);
    }
}
