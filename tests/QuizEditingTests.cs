using QuiZs.Tests.Data;
using QuiZs.Tests.Helpers;
using QuiZs.Tests.Models;
using Xunit;

namespace QuiZs.Tests;

// 3. Тестирование редактирования викторины
public sealed class QuizEditingTests : IDisposable
{
    private readonly TestDatabase _db;
    private readonly QuizRepository _repo;

    public QuizEditingTests()
    {
        _db = new TestDatabase();
        _repo = _db.Repository;
    }

    public void Dispose() => _db.Dispose();

    // 3.1 Изменение текста вопроса сохраняется
    [Fact]
    public async Task EditQuiz_ChangeQuestionText_IsPersisted()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, "Викторина");
        var updatedQuiz = new Quiz
        {
            Id = saved.Id,
            Title = saved.Title,
            Questions =
            [
                new Question
                {
                    Text = "Изменённый вопрос",
                    Answers = saved.Questions[0].Answers
                        .Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect })
                        .ToList()
                },
                new Question
                {
                    Text = saved.Questions[1].Text,
                    Answers = saved.Questions[1].Answers
                        .Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect })
                        .ToList()
                }
            ]
        };

        await _repo.SaveQuizAsync(updatedQuiz);

        var loaded = await _repo.GetQuizAsync(saved.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Изменённый вопрос", loaded.Questions[0].Text);
    }

    // 3.2 Добавление нового вопроса увеличивает их количество
    [Fact]
    public async Task EditQuiz_AddQuestion_QuestionCountIncreases()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, questionCount: 2);
        var updatedQuestions = saved.Questions
            .Select(q => new Question
            {
                Text = q.Text,
                Answers = q.Answers.Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect }).ToList()
            }).ToList();

        updatedQuestions.Add(QuizBuilder.MakeQuestion("Новый вопрос"));

        var updatedQuiz = new Quiz
        {
            Id = saved.Id,
            Title = saved.Title,
            Questions = updatedQuestions
        };
        await _repo.SaveQuizAsync(updatedQuiz);

        var list = await _repo.GetQuizListAsync();
        Assert.Equal(3, list[0].QuestionCount);
    }

    // 3.3 Удаление вопроса уменьшает их количество
    [Fact]
    public async Task EditQuiz_RemoveQuestion_QuestionCountDecreases()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, questionCount: 3);

        var updatedQuiz = new Quiz
        {
            Id = saved.Id,
            Title = saved.Title,
            Questions = saved.Questions.Take(2)
                .Select(q => new Question
                {
                    Text = q.Text,
                    Answers = q.Answers.Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect }).ToList()
                }).ToList()
        };

        await _repo.SaveQuizAsync(updatedQuiz);

        var list = await _repo.GetQuizListAsync();
        Assert.Equal(2, list[0].QuestionCount);
    }

    // 3.4 Изменение текста варианта ответа сохраняется
    [Fact]
    public async Task EditQuiz_ChangeAnswerText_IsPersisted()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo);

        var updatedQuestions = saved.Questions.Select((q, qi) => new Question
        {
            Text = q.Text,
            Answers = q.Answers.Select((a, ai) => new Answer
            {
                Text = qi == 0 && ai == 0 ? "Изменённый вариант" : a.Text,
                IsCorrect = a.IsCorrect
            }).ToList()
        }).ToList();

        var updatedQuiz = new Quiz { Id = saved.Id, Title = saved.Title, Questions = updatedQuestions };
        await _repo.SaveQuizAsync(updatedQuiz);

        var loaded = await _repo.GetQuizAsync(saved.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Изменённый вариант", loaded.Questions[0].Answers[0].Text);
    }

    // 3.5 Изменение правильного ответа сохраняется
    [Fact]
    public async Task EditQuiz_ChangeCorrectAnswer_IsPersisted()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo);
        var firstQuestion = saved.Questions[0];
        Assert.True(firstQuestion.Answers[0].IsCorrect);

        var updatedQuestions = saved.Questions.Select((q, qi) => new Question
        {
            Text = q.Text,
            Answers = q.Answers.Select((a, ai) => new Answer
            {
                Text = a.Text,
                IsCorrect = qi == 0 ? ai == 2 : a.IsCorrect
            }).ToList()
        }).ToList();

        var updatedQuiz = new Quiz { Id = saved.Id, Title = saved.Title, Questions = updatedQuestions };
        await _repo.SaveQuizAsync(updatedQuiz);

        var loaded = await _repo.GetQuizAsync(saved.Id);
        Assert.NotNull(loaded);
        Assert.False(loaded.Questions[0].Answers[0].IsCorrect);
        Assert.True(loaded.Questions[0].Answers[2].IsCorrect);
    }

    // 3.6 Изменение названия викторины сохраняется
    [Fact]
    public async Task EditQuiz_ChangeTitle_IsPersisted()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, "Старое название");

        var updatedQuiz = new Quiz
        {
            Id = saved.Id,
            Title = "Новое название",
            Questions = saved.Questions.Select(q => new Question
            {
                Text = q.Text,
                Answers = q.Answers.Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect }).ToList()
            }).ToList()
        };
        await _repo.SaveQuizAsync(updatedQuiz);

        var list = await _repo.GetQuizListAsync();
        Assert.Equal("Новое название", list[0].Title);
    }

    // 3.7 При редактировании: если новое название совпадает с другой викториной, добавляется суффикс
    [Fact]
    public async Task EditQuiz_RenameToExistingTitle_GetsNumericSuffix()
    {
        await _repo.SaveQuizAsync(QuizBuilder.MakeQuiz("Биология"));
        var quiz2 = await QuizBuilder.SavedQuizAsync(_repo, "Физика");
        var updatedQuiz = new Quiz
        {
            Id = quiz2.Id,
            Title = "Биология",
            Questions = quiz2.Questions.Select(q => new Question
            {
                Text = q.Text,
                Answers = q.Answers.Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect }).ToList()
            }).ToList()
        };
        var savedTitle = await _repo.SaveQuizAsync(updatedQuiz);

        Assert.Equal("Биология 1", savedTitle);
    }

    // 3.8 При редактировании: название не меняется - суффикс не добавляется
    [Fact]
    public async Task EditQuiz_KeepSameTitle_NoSuffixAdded()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, "Математика");

        var updatedQuiz = new Quiz
        {
            Id = saved.Id,
            Title = "Математика",
            Questions = saved.Questions.Select(q => new Question
            {
                Text = q.Text,
                Answers = q.Answers.Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect }).ToList()
            }).ToList()
        };
        var savedTitle = await _repo.SaveQuizAsync(updatedQuiz);

        Assert.Equal("Математика", savedTitle);
    }

    // 3.9 Порядок вопросов сохраняется после редактирования
    [Fact]
    public async Task EditQuiz_QuestionOrderIsPreserved()
    {
        var saved = await QuizBuilder.SavedQuizAsync(_repo, questionCount: 3);
        var expectedOrder = new[] { "Вопрос A", "Вопрос B", "Вопрос C" };

        var updatedQuiz = new Quiz
        {
            Id = saved.Id,
            Title = saved.Title,
            Questions = expectedOrder.Select(text => QuizBuilder.MakeQuestion(text)).ToList()
        };
        await _repo.SaveQuizAsync(updatedQuiz);

        var loaded = await _repo.GetQuizAsync(saved.Id);
        Assert.NotNull(loaded);
        Assert.Equal(expectedOrder, loaded.Questions.Select(q => q.Text).ToArray());
    }
}
