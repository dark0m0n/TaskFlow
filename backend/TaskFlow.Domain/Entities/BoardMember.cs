namespace TaskFlow.Domain.Entities;

public class BoardMember
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public BoardRole Role { get; set; } = BoardRole.Member;
}
