namespace TaskFlow.Domain.Entities;

public class Column
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }

    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    public List<Card> Cards { get; set; } = [];
}
