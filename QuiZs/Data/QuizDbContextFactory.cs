using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuiZs.Data;

public sealed class QuizDbContextFactory : IDesignTimeDbContextFactory<QuizDbContext>
{
    public QuizDbContext CreateDbContext(string[] args)
    {
        var databasePath = Path.Combine(Directory.GetCurrentDirectory(), "quizzes.design.db");
        var optionsBuilder = new DbContextOptionsBuilder<QuizDbContext>();
        optionsBuilder.UseSqlite($"Data Source={databasePath}");

        return new QuizDbContext(optionsBuilder.Options, databasePath);
    }
}
