using Microsoft.EntityFrameworkCore;
using QuiZs.Extensions;
using QuiZs.Models;

namespace QuiZs.Data;

public sealed class QuizRepository(Func<QuizDbContext>? contextFactory = null)
{
    private const string BinaryCollation = "BINARY";
    private const string CaseInsensitiveCollation = "NOCASE";

    private readonly Func<QuizDbContext> _contextFactory = contextFactory ?? CreateDefaultContext;

    public async Task<List<QuizListItem>> GetQuizListAsync()
    {
        await using var context = CreateContext();

        return await context.Quizzes
            .AsNoTracking()
            .OrderBy(quiz => EF.Functions.Collate(quiz.Title, CaseInsensitiveCollation))
            .Select(quiz => new QuizListItem(
                quiz.Id,
                quiz.Title,
                quiz.Questions.Count))
            .ToListAsync();
    }

    public async Task<Quiz?> GetQuizAsync(int quizId)
    {
        await using var context = CreateContext();

        var quiz = await context.Quizzes
            .AsNoTracking()
            .Include(item => item.Questions)
            .ThenInclude(item => item.Answers)
            .SingleOrDefaultAsync(item => item.Id == quizId);

        return quiz is null ? null : OrderQuizGraph(quiz);
    }

    public async Task<string> SaveQuizAsync(Quiz quiz)
    {
        ArgumentNullException.ThrowIfNull(quiz);

        await using var context = CreateContext();

        var normalizedTitle = quiz.Title.NormalizeRequiredText(nameof(quiz.Title));
        var finalTitle = await CreateUniqueTitleAsync(context, normalizedTitle, quiz.Id == 0 ? null : quiz.Id);
        var normalizedQuestions = CreateQuestionCopies(quiz.Questions);
        var now = DateTimeOffset.UtcNow;

        if (quiz.Id == 0)
        {
            context.Quizzes.Add(new Quiz
            {
                Title = finalTitle,
                CreatedAt = now,
                UpdatedAt = now,
                Questions = normalizedQuestions
            });
        }
        else
        {
            var existingQuiz = await GetQuizForUpdateAsync(context, quiz.Id);
            ReplaceQuizContent(context, existingQuiz, finalTitle, normalizedQuestions, now);
        }

        await context.SaveChangesAsync();
        return finalTitle;
    }

    public async Task DeleteQuizAsync(int quizId)
    {
        await using var context = CreateContext();

        var quiz = await context.Quizzes.FindAsync(quizId);
        if (quiz is null)
        {
            return;
        }

        context.Quizzes.Remove(quiz);
        await context.SaveChangesAsync();
    }

    private static QuizDbContext CreateDefaultContext() => new();

    private QuizDbContext CreateContext() => _contextFactory();

    private static Quiz OrderQuizGraph(Quiz quiz)
    {
        quiz.Questions = quiz.Questions
            .OrderBy(question => question.Position)
            .Select(OrderQuestionAnswers)
            .ToList();

        return quiz;
    }

    private static Question OrderQuestionAnswers(Question question)
    {
        question.Answers = question.Answers
            .OrderBy(answer => answer.Position)
            .ToList();

        return question;
    }

    private static List<Question> CreateQuestionCopies(IEnumerable<Question> questions)
    {
        return questions
            .Select((question, questionIndex) => new Question
            {
                Text = question.Text.NormalizeText(),
                Position = questionIndex,
                Answers = CreateAnswerCopies(question.Answers)
            })
            .ToList();
    }

    private static List<Answer> CreateAnswerCopies(IEnumerable<Answer> answers)
    {
        return answers
            .Select((answer, answerIndex) => new Answer
            {
                Text = answer.Text.NormalizeText(),
                Position = answerIndex,
                IsCorrect = answer.IsCorrect
            })
            .ToList();
    }

    private static void ReplaceQuizContent(
        QuizDbContext context,
        Quiz existingQuiz,
        string title,
        List<Question> questions,
        DateTimeOffset now)
    {
        existingQuiz.Title = title;
        existingQuiz.UpdatedAt = now;
        context.Questions.RemoveRange(existingQuiz.Questions);
        existingQuiz.Questions = questions;
    }

    private static async Task<Quiz> GetQuizForUpdateAsync(QuizDbContext context, int quizId)
    {
        var quiz = await context.Quizzes
            .Include(item => item.Questions)
            .ThenInclude(item => item.Answers)
            .SingleOrDefaultAsync(item => item.Id == quizId);

        return quiz ?? throw new InvalidOperationException($"Quiz with id {quizId} was not found.");
    }

    private static async Task<string> CreateUniqueTitleAsync(
        QuizDbContext context,
        string title,
        int? excludedQuizId)
    {
        var candidate = title;
        var suffix = 1;

        while (await TitleExistsExactAsync(context, candidate, excludedQuizId))
        {
            candidate = $"{title} {suffix}";
            suffix++;
        }

        return candidate;
    }

    private static async Task<bool> TitleExistsExactAsync(
        QuizDbContext context,
        string title,
        int? excludedQuizId)
    {
        var query = context.Quizzes
            .AsNoTracking()
            .Where(quiz => EF.Functions.Collate(quiz.Title, BinaryCollation) == title);

        if (excludedQuizId.HasValue)
        {
            var quizId = excludedQuizId.Value;
            query = query.Where(quiz => quiz.Id != quizId);
        }

        return await query.AnyAsync();
    }
}
