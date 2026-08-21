using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface IBoardAuthorizationService
{
    Task<bool> HasAccessAsync(int boardId, string userId, BoardRole? requiredRole = null);
}
