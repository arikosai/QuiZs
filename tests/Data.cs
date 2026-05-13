using Microsoft.EntityFrameworkCore;
using QuiZs.Tests.Models;

namespace QuiZs.Tests.Data;

public sealed partial class QuizDbContext : DbContext
{
    private readonly string _databasePath;

    public QuizDbContext(string databasePath)
    {
        _databasePath = databasePath;
    }

    public QuizDbContext(DbContextOptions<QuizDbContext> options, string databasePath = ":memory:")
        : base(options)
    {
        _databasePath = databasePath;
    }

    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        optionsBuilder.UseSqlite($"Data Source={_databasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.ToTable(nameof(Quiz));
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Id).HasColumnName(nameof(Quiz.Id));
            entity.Property(q => q.Title).HasColumnName(nameof(Quiz.Title)).IsRequired();
            entity.Property(q => q.CreatedAt).HasColumnName(nameof(Quiz.CreatedAt));
            entity.Property(q => q.UpdatedAt).HasColumnName(nameof(Quiz.UpdatedAt));
            entity.HasMany(q => q.Questions)
                  .WithOne(q => q.Quiz)
                  .HasForeignKey(q => q.QuizId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable(nameof(Question));
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Id).HasColumnName(nameof(Question.Id));
            entity.Property(q => q.QuizId).HasColumnName(nameof(Question.QuizId));
            entity.Property(q => q.Text).HasColumnName(nameof(Question.Text)).IsRequired();
            entity.Property(q => q.Position).HasColumnName(nameof(Question.Position));
            entity.HasIndex(q => new { q.QuizId, q.Position });
            entity.HasMany(q => q.Answers)
                  .WithOne(a => a.Question)
                  .HasForeignKey(a => a.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Answer>(entity =>
        {
            entity.ToTable(nameof(Answer));
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName(nameof(Answer.Id));
            entity.Property(a => a.QuestionId).HasColumnName(nameof(Answer.QuestionId));
            entity.Property(a => a.Text).HasColumnName(nameof(Answer.Text)).IsRequired();
            entity.Property(a => a.Position).HasColumnName(nameof(Answer.Position));
            entity.Property(a => a.IsCorrect).HasColumnName(nameof(Answer.IsCorrect));
            entity.HasIndex(a => new { a.QuestionId, a.Position });
        });
    }
}

public sealed class QuizRepository(Func<QuizDbContext>? contextFactory = null)
{
    private const string BinaryCollation = "BINARY";
    private const string CaseInsensitiveCollation = "NOCASE";

    private readonly Func<QuizDbContext> _contextFactory =
        contextFactory ?? (() => new QuizDbContext(":memory:"));

    public async Task<List<QuizListItem>> GetQuizListAsync()
    {
        await using var context = _contextFactory();
        return await context.Quizzes
            .AsNoTracking()
            .OrderBy(quiz => EF.Functions.Collate(quiz.Title, CaseInsensitiveCollation))
            .Select(quiz => new QuizListItem(quiz.Id, quiz.Title, quiz.Questions.Count))
            .ToListAsync();
    }

    public async Task<Quiz?> GetQuizAsync(int quizId)
    {
        await using var context = _contextFactory();
        var quiz = await context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .SingleOrDefaultAsync(q => q.Id == quizId);

        return quiz is null ? null : OrderQuizGraph(quiz);
    }

    public async Task<string> SaveQuizAsync(Quiz quiz)
    {
        ArgumentNullException.ThrowIfNull(quiz);

        await using var context = _contextFactory();

        var normalizedTitle = NormalizeRequiredText(quiz.Title, nameof(quiz.Title));
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
        await using var context = _contextFactory();
        var quiz = await context.Quizzes.FindAsync(quizId);
        if (quiz is null) return;
        context.Quizzes.Remove(quiz);
        await context.SaveChangesAsync();
    }

    // Вспомогательные методы
    private static Quiz OrderQuizGraph(Quiz quiz)
    {
        quiz.Questions = quiz.Questions
            .OrderBy(q => q.Position)
            .Select(OrderQuestionAnswers)
            .ToList();
        return quiz;
    }

    private static Question OrderQuestionAnswers(Question question)
    {
        question.Answers = question.Answers.OrderBy(a => a.Position).ToList();
        return question;
    }

    private static List<Question> CreateQuestionCopies(IEnumerable<Question> questions)
    {
        return questions
            .Select((q, i) => new Question
            {
                Text = q.Text.Trim(),
                Position = i,
                Answers = CreateAnswerCopies(q.Answers)
            }).ToList();
    }

    private static List<Answer> CreateAnswerCopies(IEnumerable<Answer> answers)
    {
        return answers
            .Select((a, i) => new Answer
            {
                Text = a.Text.Trim(),
                Position = i,
                IsCorrect = a.IsCorrect
            }).ToList();
    }

    private static void ReplaceQuizContent(
        QuizDbContext context, Quiz existingQuiz,
        string title, List<Question> questions, DateTimeOffset now)
    {
        existingQuiz.Title = title;
        existingQuiz.UpdatedAt = now;
        context.Questions.RemoveRange(existingQuiz.Questions);
        existingQuiz.Questions = questions;
    }

    private static async Task<Quiz> GetQuizForUpdateAsync(QuizDbContext context, int quizId)
    {
        var quiz = await context.Quizzes
            .Include(q => q.Questions).ThenInclude(q => q.Answers)
            .SingleOrDefaultAsync(q => q.Id == quizId);
        return quiz ?? throw new InvalidOperationException($"Quiz with id {quizId} was not found.");
    }

    private static async Task<string> CreateUniqueTitleAsync(
        QuizDbContext context, string title, int? excludedQuizId)
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
        QuizDbContext context, string title, int? excludedQuizId)
    {
        var query = context.Quizzes
            .AsNoTracking()
            .Where(q => EF.Functions.Collate(q.Title, BinaryCollation) == title);

        if (excludedQuizId.HasValue)
        {
            var id = excludedQuizId.Value;
            query = query.Where(q => q.Id != id);
        }

        return await query.AnyAsync();
    }

    private static string NormalizeRequiredText(string value, string paramName)
    {
        var trimmed = value.Trim();
        return trimmed.Length == 0
            ? throw new ArgumentException("Value cannot be empty or whitespace.", paramName)
            : trimmed;
    }
}
