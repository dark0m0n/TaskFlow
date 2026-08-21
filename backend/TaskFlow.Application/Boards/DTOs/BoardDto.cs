using TaskFlow.Application.Columns.DTOs;

namespace TaskFlow.Application.Boards.DTOs;

public class BoardDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public List<ColumnDto> Columns { get; set; } = [];
    public List<BoardMemberDto> Members { get; set; } = [];
}
