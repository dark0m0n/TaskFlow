namespace TaskFlow.Domain.Entities;

public class Board
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<Column> Columns { get; set; } = [];
    public List<BoardMember> Members { get; set; } = [];
    public List<ActivityLog> ActivityLogs { get; set; } = [];
}
