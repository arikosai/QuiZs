namespace QuiZs.Models;

public sealed class DraftQuestion
{
    public const int AnswerCount = 4;

    public string Text { get; set; } = string.Empty;

    public string[] Answers { get; } = new string[AnswerCount];

    public int CorrectAnswerIndex { get; set; }
}
