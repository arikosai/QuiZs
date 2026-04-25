using QuiZs.Data;
using QuiZs.Mappers;
using QuiZs.Models;

namespace QuiZs.Pages;

public sealed partial class QuizEditorPage : ContentPage
{
    private readonly QuizRepository _repository = new();
    private readonly List<DraftQuestion> _questions = [];
    private readonly int? _quizId;

    private int _selectedQuestionIndex;
    private bool _loaded;
    private string _title = string.Empty;

    public QuizEditorPage(int? quizId = null)
    {
        _quizId = quizId;
        Title = quizId is null ? "Создание викторины" : "Редактирование викторины";
        Content = LoadingView();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_quizId is null)
        {
            var quizCount = (await _repository.GetQuizListAsync()).Count;
            _title = $"Викторина {quizCount + 1}";
            _questions.Clear();
            _questions.Add(new DraftQuestion());
            _selectedQuestionIndex = 0;
            Build();
            return;
        }

        var quiz = await _repository.GetQuizAsync(_quizId.Value);
        if (quiz is null)
        {
            await DisplayAlertAsync("Викторина не найдена", "Запись уже была удалена.", "Закрыть");
            await Navigation.PopToRootAsync();
            return;
        }

        _title = quiz.Title;
        _questions.Clear();
        _questions.AddRange(quiz.Questions.Select(DraftQuestionMapper.FromQuestion));

        if (_questions.Count == 0)
        {
            _questions.Add(new DraftQuestion());
        }

        _selectedQuestionIndex = 0;
        Build();
    }

    private DraftQuestion SelectedQuestion => _questions[_selectedQuestionIndex];

    private void UpdateTitle(string? title)
    {
        _title = title ?? string.Empty;
    }

    private void SelectQuestion(int questionIndex)
    {
        _selectedQuestionIndex = questionIndex;
        Build();
    }

    private void UpdateQuestionText(string? questionText)
    {
        SelectedQuestion.Text = questionText ?? string.Empty;
    }

    private void SetCorrectAnswer(int answerIndex)
    {
        SelectedQuestion.CorrectAnswerIndex = answerIndex;
    }

    private void UpdateAnswerText(int answerIndex, string? answerText)
    {
        SelectedQuestion.Answers[answerIndex] = answerText ?? string.Empty;
    }

    private async Task CancelAsync()
    {
        await Navigation.PopToRootAsync();
    }

    private async Task AddQuestionAsync()
    {
        if (!ValidateQuestion(_questions[_selectedQuestionIndex], _selectedQuestionIndex, out var message))
        {
            await DisplayAlertAsync("Проверьте вопрос", message, "Закрыть");
            return;
        }

        _questions.Add(new DraftQuestion());
        _selectedQuestionIndex = _questions.Count - 1;
        Build();
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_title))
        {
            await DisplayAlertAsync("Не указано название", "Введите название викторины.", "Закрыть");
            return;
        }

        if (_questions.Count < 2)
        {
            await DisplayAlertAsync("Недостаточно вопросов", "Для сохранения викторины нужно добавить не менее двух вопросов.", "Закрыть");
            return;
        }

        for (var i = 0; i < _questions.Count; i++)
        {
            if (ValidateQuestion(_questions[i], i, out var message))
            {
                continue;
            }

            _selectedQuestionIndex = i;
            Build();
            await DisplayAlertAsync("Проверьте вопрос", message, "Закрыть");
            return;
        }

        var quiz = new Quiz
        {
            Id = _quizId ?? 0,
            Title = _title.Trim(),
            Questions = _questions.Select(DraftQuestionMapper.ToQuestion).ToList()
        };

        var savedTitle = await _repository.SaveQuizAsync(quiz);
        if (savedTitle != _title.Trim())
        {
            await DisplayAlertAsync("Название изменено", $"Викторина сохранена как «{savedTitle}».", "Закрыть");
        }

        await Navigation.PopToRootAsync();
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
