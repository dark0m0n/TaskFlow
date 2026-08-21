using MediatR;
using TaskFlow.Application.Users.DTOs;

namespace TaskFlow.Application.Users.Queries.SearchUserByEmail;

public record SearchUserByEmailQuery(string EmailQuery) : IRequest<List<UserDto>>;
