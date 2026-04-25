using Microsoft.EntityFrameworkCore;
using QuiZs.Models;

namespace QuiZs.Data;

public sealed partial class QuizDbContext : DbContext
{
    private readonly string _databasePath;

    public QuizDbContext()
    {
        _databasePath = GetRuntimeDatabasePath();
    }

    public QuizDbContext(DbContextOptions<QuizDbContext> options, string? databasePath = null)
        : base(options)
    {
        _databasePath = databasePath ?? GetRuntimeDatabasePath();
    }

    public DbSet<Quiz> Quizzes => Set<Quiz>();

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<Answer> Answers => Set<Answer>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        optionsBuilder.UseSqlite($"Data Source={_databasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.ToTable(nameof(Quiz));
            entity.HasKey(quiz => quiz.Id);

            entity.Property(quiz => quiz.Id).HasColumnName(nameof(Quiz.Id));
            entity.Property(quiz => quiz.Title).HasColumnName(nameof(Quiz.Title)).IsRequired();
            entity.Property(quiz => quiz.CreatedAt).HasColumnName(nameof(Quiz.CreatedAt));
            entity.Property(quiz => quiz.UpdatedAt).HasColumnName(nameof(Quiz.UpdatedAt));

            entity.HasMany(quiz => quiz.Questions)
                .WithOne(question => question.Quiz)
                .HasForeignKey(question => question.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable(nameof(Question));
            entity.HasKey(question => question.Id);

            entity.Property(question => question.Id).HasColumnName(nameof(Question.Id));
            entity.Property(question => question.QuizId).HasColumnName(nameof(Question.QuizId));
            entity.Property(question => question.Text).HasColumnName(nameof(Question.Text)).IsRequired();
            entity.Property(question => question.Position).HasColumnName(nameof(Question.Position));

            entity.HasIndex(question => new { question.QuizId, question.Position });

            entity.HasMany(question => question.Answers)
                .WithOne(answer => answer.Question)
                .HasForeignKey(answer => answer.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Answer>(entity =>
        {
            entity.ToTable(nameof(Answer));
            entity.HasKey(answer => answer.Id);

            entity.Property(answer => answer.Id).HasColumnName(nameof(Answer.Id));
            entity.Property(answer => answer.QuestionId).HasColumnName(nameof(Answer.QuestionId));
            entity.Property(answer => answer.Text).HasColumnName(nameof(Answer.Text)).IsRequired();
            entity.Property(answer => answer.Position).HasColumnName(nameof(Answer.Position));
            entity.Property(answer => answer.IsCorrect).HasColumnName(nameof(Answer.IsCorrect));

            entity.HasIndex(answer => new { answer.QuestionId, answer.Position });
        });
    }

    private static string GetRuntimeDatabasePath() => Path.Combine(FileSystem.AppDataDirectory, "quizzes.db");
}
