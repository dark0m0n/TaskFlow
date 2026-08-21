using TaskFlow.Application.Cards.DTOs;

namespace TaskFlow.Application.Columns.DTOs;

public class ColumnDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<CardDto> Cards { get; set; } = [];
}
