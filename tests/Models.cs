namespace QuiZs.Tests.Models;

public sealed class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<Question> Questions { get; set; } = [];
}

public sealed class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Position { get; set; }
    public Quiz? Quiz { get; set; }
    public List<Answer> Answers { get; set; } = [];
}

public sealed class Answer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool IsCorrect { get; set; }
    public Question? Question { get; set; }
}

public sealed class DraftQuestion
{
    public const int AnswerCount = 4;
    public string Text { get; set; } = string.Empty;
    public string[] Answers { get; } = new string[AnswerCount];
    public int CorrectAnswerIndex { get; set; }
}

public sealed record QuizListItem(int Id, string Title, int QuestionCount);

public enum QuizRunMode
{
    Demo,
    Pass
}
