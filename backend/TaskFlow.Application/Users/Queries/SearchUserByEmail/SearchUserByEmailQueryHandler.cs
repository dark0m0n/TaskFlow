using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Users.DTOs;

namespace TaskFlow.Application.Users.Queries.SearchUserByEmail;

public class SearchUserByEmailQueryHandler(IApplicationDbContext context)
    : IRequestHandler<SearchUserByEmailQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<List<UserDto>> Handle(SearchUserByEmailQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EmailQuery))
            return [];

        var query = request.EmailQuery.Trim();

        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Email != null && u.Email.Contains(query, StringComparison.CurrentCultureIgnoreCase))
            .Take(10)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email!,
                UserName = u.UserName ?? u.Email!
            })
            .ToListAsync();
    }
}
