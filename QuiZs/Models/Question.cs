namespace QuiZs.Models;

public sealed class Question
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string Text { get; set; } = string.Empty;

    public int Position { get; set; }

    public Quiz? Quiz { get; set; }

    public List<Answer> Answers { get; set; } = [];
}
