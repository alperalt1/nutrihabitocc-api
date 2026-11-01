using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Persistence.Repositories;

public class WorkspaceRepository: IWorkspaceRepository
{
    private readonly AppDbContext _context;

    public WorkspaceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<Workspace>> GetAllAsync()
    {
        return await _context.Workspaces
            .AsNoTracking()
            .Where(workspace => workspace.IsActive)
            .OrderBy(workspace => workspace.Name)
            .ToListAsync();
    }

    public async Task<ICollection<Workspace>> GetAllForUserAsync(int userId)
    {
        return await _context.Workspaces
            .AsNoTracking()
            .Where(workspace =>
                workspace.IsActive &&
                workspace.WorkspaceMembers.Any(member =>
                    member.UserId == userId &&
                    member.IsActive &&
                    member.MemberRole == AppRoles.Nutricionista))
            .OrderBy(workspace => workspace.Name)
            .ToListAsync();
    }

    public async Task<Workspace?> GetByIdAsync(int id)
    {
        return await _context.Workspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(workspace => workspace.Id == id && workspace.IsActive);
    }

    public async Task<Workspace> CreateAsync(Workspace workspace)
    {
        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();
        return workspace;
    }

    public async Task<bool> UpdateAsync(Workspace workspace)
    {
        var existingWorkspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == workspace.Id && w.IsActive);

        if (existingWorkspace is null)
        {
            return false;
        }

        if (existingWorkspace.Name != workspace.Name ||
            existingWorkspace.Description != workspace.Description)
        {
            existingWorkspace.Name = workspace.Name;
            existingWorkspace.Description = workspace.Description;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == id && w.IsActive);

        if (workspace is null)
        {
            return false;
        }

        _context.Workspaces.Remove(workspace);
        return await _context.SaveChangesAsync() > 0;
    }
}
