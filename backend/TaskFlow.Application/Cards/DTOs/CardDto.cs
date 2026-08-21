using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Cards.DTOs;

public class CardDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PriorityLevel Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public int Order { get; set; }
    public string? AssigneeId { get; set; }
}
