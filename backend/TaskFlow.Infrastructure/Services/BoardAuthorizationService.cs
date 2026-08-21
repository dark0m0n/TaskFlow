using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Services;

public class BoardAuthorizationService(IApplicationDbContext context) : IBoardAuthorizationService
{
    private readonly IApplicationDbContext _context = context;

    public async Task<bool> HasAccessAsync(int boardId, string userId, BoardRole? requiredRole = null)
    {
        var member = await _context.BoardMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == userId);

        if (member == null) return false;

        if (requiredRole.HasValue)
            return member.Role <= requiredRole.Value;

        return true;
    }
}