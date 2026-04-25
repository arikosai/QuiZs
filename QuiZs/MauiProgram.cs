using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuiZs.Data;

namespace QuiZs;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        ApplyDatabaseMigrations();

        return app;
    }

    private static void ApplyDatabaseMigrations()
    {
        using var context = new QuizDbContext();
        context.Database.Migrate();
    }
}
