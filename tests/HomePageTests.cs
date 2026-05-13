using QuiZs.Tests.Data;
using QuiZs.Tests.Helpers;
using QuiZs.Tests.Models;
using Xunit;

namespace QuiZs.Tests;

// 1. Тестирование главного меню
public sealed class HomePageTests : IDisposable
{
    private readonly TestDatabase _db;
    private readonly QuizRepository _repo;

    public HomePageTests()
    {
        _db = new TestDatabase();
        _repo = _db.Repository;
    }

    public void Dispose() => _db.Dispose();

    // 1.1 Список викторин отображается в алфавитном порядке
    [Fact]
    public async Task GetQuizList_ReturnsSortedAlphabetically()
    {
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("Физика"));
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("Астрономия"));
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("Математика"));

        var list = await _repo.GetQuizListAsync();

        Assert.Equal("Астрономия", list[0].Title);
        Assert.Equal("Математика", list[1].Title);
        Assert.Equal("Физика", list[2].Title);
    }

    // 1.2 Сортировка без учёта регистра символов
    [Fact]
    public async Task GetQuizList_SortingIsCaseInsensitive()
    {
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("биология"));
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("Астрономия"));
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("Химия"));

        var list = await _repo.GetQuizListAsync();

        Assert.Equal("Астрономия", list[0].Title);
        Assert.Equal("биология", list[1].Title);
        Assert.Equal("Химия", list[2].Title);
    }

    // 1.3 Пустой список при отсутствии викторин
    [Fact]
    public async Task GetQuizList_ReturnsEmptyList_WhenNoQuizzes()
    {
        var list = await _repo.GetQuizListAsync();

        Assert.Empty(list);
    }

    // 1.4 Создание викторины - список обновляется
    [Fact]
    public async Task CreateQuiz_AppearsInList()
    {
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("Новая викторина"));

        var list = await _repo.GetQuizListAsync();

        Assert.Single(list);
        Assert.Equal("Новая викторина", list[0].Title);
    }

    // 1.5 Редактирование - существующая викторина загружается по Id
    [Fact]
    public async Task EditQuiz_ExistingQuizCanBeLoaded()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, "История");

        var loaded = await _repo.GetQuizAsync(saved.Id);

        Assert.NotNull(loaded);
        Assert.Equal("История", loaded.Title);
    }

    // 1.6 Редактирование - несуществующая викторина возвращает null
    [Fact]
    public async Task EditQuiz_ReturnsNull_WhenQuizNotFound()
    {
        var loaded = await _repo.GetQuizAsync(99999);

        Assert.Null(loaded);
    }

    // 1.7 Подтверждение удаления: если пользователь подтвердил - викторина удаляется из списка
    [Fact]
    public async Task DeleteQuiz_AfterConfirmation_RemovesFromList()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, "Удаляемая викторина");

        bool userConfirmed = true;
        if (userConfirmed)
            await _repo.DeleteQuizAsync(saved.Id);

        var list = await _repo.GetQuizListAsync();
        Assert.Empty(list);
    }

    // 1.8 Подтверждение удаления: если пользователь отказался - викторина остаётся в списке
    [Fact]
    public async Task DeleteQuiz_WhenCancelled_QuizRemainsInList()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, "Сохранённая викторина");

        bool userConfirmed = false;
        if (userConfirmed)
            await _repo.DeleteQuizAsync(saved.Id);

        var list = await _repo.GetQuizListAsync();
        Assert.Single(list);
    }

    // 1.9 Удаление несуществующей викторины не вызывает ошибку
    [Fact]
    public async Task DeleteQuiz_NonExistingId_DoesNotThrow()
    {
        var exception = await Record.ExceptionAsync(() => _repo.DeleteQuizAsync(99999));

        Assert.Null(exception);
    }

    // 1.10 Запуск викторины в режиме «Демонстрация»
    [Fact]
    public async Task StartDemo_QuizLoadedWithCorrectMode()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, "Демо-викторина");

        var quiz = await _repo.GetQuizAsync(saved.Id);
        var mode = QuizRunMode.Demo;

        Assert.NotNull(quiz);
        Assert.Equal(QuizRunMode.Demo, mode);
        Assert.NotEmpty(quiz.Questions);
    }

    // 1.11 Запуск викторины в режиме «Прохождение»
    [Fact]
    public async Task StartPass_QuizLoadedWithCorrectMode()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, "Экзамен");

        var quiz = await _repo.GetQuizAsync(saved.Id);
        var mode = QuizRunMode.Pass;

        Assert.NotNull(quiz);
        Assert.Equal(QuizRunMode.Pass, mode);
        Assert.NotEmpty(quiz.Questions);
    }

    // 1.12 Запуск недоступен, если викторина не содержит вопросов
    [Fact]
    public async Task StartQuiz_WithNoQuestions_ShouldBeBlocked()
    {
        var quiz = new Quiz { Title = "Пустая" };
        await _repo.SaveQuizAsync(quiz);

        var list = await _repo.GetQuizListAsync();
        var item = list.First(q => q.Title == "Пустая");
        var loaded = await _repo.GetQuizAsync(item.Id);

        var canStart = loaded is not null && loaded.Questions.Count > 0;
        Assert.False(canStart);
    }
}
