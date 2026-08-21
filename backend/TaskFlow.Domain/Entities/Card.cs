namespace TaskFlow.Domain.Entities;

public class Card
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public DateTime? DueDate { get; set; }
    public int Order { get; set; }

    public int ColumnId { get; set; }
    public Column Column { get; set; } = null!;

    public string? AssigneeId { get; set; }
    public List<Comment> Comments { get; set; } = [];
}
