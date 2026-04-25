namespace QuiZs.Models;

public sealed class Quiz
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<Question> Questions { get; set; } = [];
}
