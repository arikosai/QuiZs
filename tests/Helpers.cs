using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuiZs.Tests.Data;
using QuiZs.Tests.Models;

namespace QuiZs.Tests.Helpers;

public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    public Func<QuizDbContext> Factory { get; }
    public QuizRepository Repository { get; }

    public TestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _connection.CreateCollation("NOCASE", (x, y) =>
            string.Compare(x, y, StringComparison.OrdinalIgnoreCase));

        Factory = () =>
        {
            var options = new DbContextOptionsBuilder<QuizDbContext>()
                .UseSqlite(_connection)
                .Options;
            return new QuizDbContext(options, ":memory:");
        };

        using var ctx = Factory();
        ctx.Database.EnsureCreated();

        Repository = new QuizRepository(Factory);
    }

    public void Dispose() => _connection.Dispose();
}

public static class QuizBuilder
{
    // Создаёт вопрос с 4 вариантами ответа
    public static Question MakeQuestion(string text = "Вопрос?", int correctIndex = 0)
    {
        var answers = Enumerable.Range(0, DraftQuestion.AnswerCount)
            .Select((i) => new Answer
            {
                Text = $"Вариант {i + 1}",
                Position = i,
                IsCorrect = i == correctIndex
            }).ToList();

        return new Question { Text = text, Answers = answers };
    }

    // Создаёт викторину с заданным количеством вопросов
    public static Quiz MakeQuiz(string title = "Тестовая викторина", int questionCount = 2)
    {
        return new Quiz
        {
            Title = title,
            Questions = Enumerable.Range(1, questionCount)
                .Select(i => MakeQuestion($"Вопрос {i}"))
                .ToList()
        };
    }

    // Сохраняет викторину в БД и возвращает сохранённый объект с Id
    public static async Task<Quiz> SavedQuizAsync(
        QuizRepository repo, string title = "Тестовая викторина", int questionCount = 2)
    {
        var quiz = MakeQuiz(title, questionCount);
        await repo.SaveQuizAsync(quiz);
        var list = await repo.GetQuizListAsync();
        var item = list.First(q => q.Title == title);
        return (await repo.GetQuizAsync(item.Id))!;
    }
}
