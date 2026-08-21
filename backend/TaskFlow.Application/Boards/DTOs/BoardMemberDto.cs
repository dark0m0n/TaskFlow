using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Boards.DTOs;

public class BoardMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public BoardRole Role { get; set; }
}