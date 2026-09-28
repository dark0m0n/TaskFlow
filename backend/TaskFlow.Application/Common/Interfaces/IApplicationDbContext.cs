using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Board> Boards { get; }
    DbSet<BoardMember> BoardMembers { get; }
    DbSet<Column> Columns { get; }
    DbSet<Card> Cards { get; }
    DbSet<Comment> Comments { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<AppUser> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}