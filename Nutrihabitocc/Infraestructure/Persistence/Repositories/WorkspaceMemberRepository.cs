using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Persistence.Repositories;

public class WorkspaceMemberRepository: IWorkspaceMemberRepository
{
    private readonly AppDbContext _context;

    public WorkspaceMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<WorkspaceMember>> GetByWorkspaceAsync(int workspaceId)
    {
        return await _context.WorkspaceMembers
            .AsNoTracking()
            .Include(member => member.User)
            .Include(member => member.Workspace)
            .Where(member => member.WorkspaceId == workspaceId && member.IsActive)
            .OrderBy(member => member.CreatedOn)
            .ToListAsync();
    }

    public async Task<WorkspaceMember?> GetByIdAsync(int id)
    {
        return await _context.WorkspaceMembers
            .AsNoTracking()
            .Include(member => member.User)
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(member => member.Id == id && member.IsActive);
    }

    public async Task<WorkspaceMember?> GetActiveByUserAndWorkspaceAsync(int userId, int workspaceId)
    {
        return await _context.WorkspaceMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(member =>
                member.UserId == userId &&
                member.WorkspaceId == workspaceId &&
                member.IsActive);
    }

    public async Task<bool> IsActiveMemberAsync(int userId, int workspaceId, string memberRole)
    {
        var normalizedRole = memberRole.Trim().ToLower();
        return await _context.WorkspaceMembers
            .AsNoTracking()
            .AnyAsync(member =>
                member.UserId == userId &&
                member.WorkspaceId == workspaceId &&
                member.IsActive &&
                member.MemberRole.ToLower() == normalizedRole);
    }

    public async Task<ICollection<int>> GetActiveWorkspaceIdsAsync(int userId, string memberRole)
    {
        var normalizedRole = memberRole.Trim().ToLower();
        return await _context.WorkspaceMembers
            .AsNoTracking()
            .Where(member =>
                member.UserId == userId &&
                member.IsActive &&
                member.MemberRole.ToLower() == normalizedRole)
            .Select(member => member.WorkspaceId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<int> CountActiveByRoleAsync(int workspaceId, string memberRole)
    {
        var normalizedRole = memberRole.Trim().ToLower();
        return await _context.WorkspaceMembers
            .CountAsync(member =>
                member.WorkspaceId == workspaceId &&
                member.IsActive &&
                member.MemberRole.ToLower() == normalizedRole);
    }

    public async Task<WorkspaceMember> CreateAsync(WorkspaceMember workspaceMember)
    {
        _context.WorkspaceMembers.Add(workspaceMember);
        await _context.SaveChangesAsync();
        return workspaceMember;
    }

    public async Task<bool> UpdateAsync(WorkspaceMember workspaceMember)
    {
        var existingMember = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(member => member.Id == workspaceMember.Id && member.IsActive);

        if (existingMember is null)
        {
            return false;
        }

        if (existingMember.MemberRole != workspaceMember.MemberRole)
        {
            existingMember.MemberRole = workspaceMember.MemberRole;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var member = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(member => member.Id == id && member.IsActive);

        if (member is null)
        {
            return false;
        }

        _context.WorkspaceMembers.Remove(member);
        return await _context.SaveChangesAsync() > 0;
    }
}
