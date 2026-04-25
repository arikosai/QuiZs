namespace QuiZs.Models;

public sealed class Answer
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string Text { get; set; } = string.Empty;

    public int Position { get; set; }

    public bool IsCorrect { get; set; }

    public Question? Question { get; set; }
}
