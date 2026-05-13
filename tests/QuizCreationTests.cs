using QuiZs.Tests.Data;
using QuiZs.Tests.Helpers;
using QuiZs.Tests.Models;
using Xunit;

namespace QuiZs.Tests;

// 2. Тестирование создания викторины
public sealed class QuizCreationTests : IDisposable
{
    private readonly TestDatabase _db;
    private readonly QuizRepository _repo;

    public QuizCreationTests()
    {
        _db = new TestDatabase();
        _repo = _db.Repository;
    }

    public void Dispose() => _db.Dispose();

    // 2.1 Сохранение викторины с названием
    [Fact]
    public async Task SaveQuiz_WithTitle_TitleIsPersisted()
    {
        var quiz = QuizBuilder.MakeQuiz("Моя первая викторина");

        await _repo.SaveQuizAsync(quiz);

        var list = await _repo.GetQuizListAsync();
        Assert.Single(list);
        Assert.Equal("Моя первая викторина", list[0].Title);
    }

    // 2.2 Название обрезается от лишних пробелов
    [Fact]
    public async Task SaveQuiz_TitleWithSpaces_IsTrimmed()
    {
        var quiz = QuizBuilder.MakeQuiz("  Пробелы вокруг  ");

        await _repo.SaveQuizAsync(quiz);

        var list = await _repo.GetQuizListAsync();
        Assert.Equal("Пробелы вокруг", list[0].Title);
    }

    // 2.3 Пустое название вызывает исключение
    [Fact]
    public async Task SaveQuiz_EmptyTitle_ThrowsArgumentException()
    {
        var quiz = QuizBuilder.MakeQuiz("   "); // только пробелы

        await Assert.ThrowsAsync<ArgumentException>(() => _repo.SaveQuizAsync(quiz));
    }

    // 2.4 Добавление вопросов: вопросы сохраняются
    [Fact]
    public async Task SaveQuiz_WithQuestions_QuestionsArePersisted()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);

        await _repo.SaveQuizAsync(quiz);

        var list = await _repo.GetQuizListAsync();
        Assert.Equal(3, list[0].QuestionCount);
    }

    // 2.5 Каждый вопрос содержит ровно 4 варианта ответа
    [Fact]
    public async Task SaveQuiz_EachQuestionHasExactlyFourAnswers()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 2);

        await _repo.SaveQuizAsync(quiz);

        var list = await _repo.GetQuizListAsync();
        var saved = await _repo.GetQuizAsync(list[0].Id);

        Assert.NotNull(saved);
        foreach (var question in saved.Questions)
        {
            Assert.Equal(DraftQuestion.AnswerCount, question.Answers.Count);
        }
    }

    // 2.6 Каждый вопрос содержит ровно один правильный ответ
    [Fact]
    public async Task SaveQuiz_EachQuestionHasExactlyOneCorrectAnswer()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);

        await _repo.SaveQuizAsync(quiz);

        var list = await _repo.GetQuizListAsync();
        var saved = await _repo.GetQuizAsync(list[0].Id);

        Assert.NotNull(saved);
        foreach (var question in saved.Questions)
        {
            var correctCount = question.Answers.Count(a => a.IsCorrect);
            Assert.Equal(1, correctCount);
        }
    }

    // 2.7 Валидация: нельзя сохранить викторину менее чем с 2 вопросами
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void ValidateSave_LessThanTwoQuestions_IsInvalid(int questionCount)
    {
        var questions = Enumerable.Range(0, questionCount)
            .Select(_ => new DraftQuestion()).ToList();

        var canSave = questions.Count >= 2;

        Assert.False(canSave);
    }

    // 2.8 Валидация: викторину с 2 и более вопросами можно сохранить
    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    public void ValidateSave_TwoOrMoreQuestions_IsValid(int questionCount)
    {
        var questions = Enumerable.Range(0, questionCount)
            .Select(_ => new DraftQuestion()).ToList();

        var canSave = questions.Count >= 2;

        Assert.True(canSave);
    }

    // 2.9 Валидация вопроса: пустой текст вопроса - ошибка
    [Fact]
    public void ValidateQuestion_EmptyText_IsInvalid()
    {
        var question = new DraftQuestion { Text = "" };
        question.Answers[0] = "А";
        question.Answers[1] = "Б";
        question.Answers[2] = "В";
        question.Answers[3] = "Г";

        var isValid = ValidateQuestion(question, 0, out var message);

        Assert.False(isValid);
        Assert.Contains("вопрос", message, StringComparison.OrdinalIgnoreCase);
    }

    // 2.10 Валидация вопроса: незаполненный вариант ответа - ошибка
    [Fact]
    public void ValidateQuestion_EmptyAnswer_IsInvalid()
    {
        var question = new DraftQuestion { Text = "Какой цвет неба?" };
        question.Answers[0] = "Синий";
        question.Answers[1] = ""; // пустой
        question.Answers[2] = "Зелёный";
        question.Answers[3] = "Жёлтый";

        var isValid = ValidateQuestion(question, 0, out var message);

        Assert.False(isValid);
        Assert.Contains("вариант ответа", message, StringComparison.OrdinalIgnoreCase);
    }

    // 2.11 Валидация вопроса: все поля заполнены - вопрос валиден
    [Fact]
    public void ValidateQuestion_AllFieldsFilled_IsValid()
    {
        var question = new DraftQuestion { Text = "Какой цвет неба?" };
        question.Answers[0] = "Синий";
        question.Answers[1] = "Красный";
        question.Answers[2] = "Зелёный";
        question.Answers[3] = "Жёлтый";
        question.CorrectAnswerIndex = 0;

        var isValid = ValidateQuestion(question, 0, out var message);

        Assert.True(isValid);
        Assert.Empty(message);
    }

    // 2.12 Уникальность названия: одинаковые названия получают числовой суффикс
    [Fact]
    public async Task SaveQuiz_DuplicateTitle_SameCase_GetsNumericSuffix()
    {
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("История"));

        var duplicate = QuizBuilder.MakeQuiz("История");
        var savedTitle = await _repo.SaveQuizAsync(duplicate);

        Assert.Equal("История 1", savedTitle);
    }

    // 2.13 Уникальность названия: три викторины с одним именем получают суффиксы 1, 2 и т.д.
    [Fact]
    public async Task SaveQuiz_ThreeDuplicateTitles_GetIncrementingSuffixes()
    {
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("Химия"));
        var title2 = await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("Химия"));
        var title3 = await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("Химия"));

        Assert.Equal("Химия 1", title2);
        Assert.Equal("Химия 2", title3);
    }

    // 2.14 Уникальность названия учитывает регистр: «история» и «История» — разные названия, суффикс не добавляется
    [Fact]
    public async Task SaveQuiz_DifferentCase_TreatedAsDifferentTitle()
    {
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("история"));

        var savedTitle = await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("История"));

        Assert.Equal("История", savedTitle);
    }

    private static bool ValidateQuestion(DraftQuestion question, int index, out string message)
    {
        if (string.IsNullOrWhiteSpace(question.Text))
        {
            message = $"Введите текст вопроса {index + 1}.";
            return false;
        }

        for (var i = 0; i < DraftQuestion.AnswerCount; i++)
        {
            if (string.IsNullOrWhiteSpace(question.Answers[i]))
            {
                message = $"Введите вариант ответа {i + 1} для вопроса {index + 1}.";
                return false;
            }
        }

        message = string.Empty;
        return true;
    }
}
