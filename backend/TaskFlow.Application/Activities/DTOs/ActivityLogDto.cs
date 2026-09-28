namespace TaskFlow.Application.Activities.DTOs;

public class ActivityLogDto
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
