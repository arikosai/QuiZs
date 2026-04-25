using QuiZs.Data;
using QuiZs.Models;

namespace QuiZs.Pages;

public sealed partial class HomePage : ContentPage
{
    private readonly QuizRepository _repository = new();
    private readonly List<QuizListItem> _quizzes = [];

    private int? _openMenuQuizId;
    private Point? _menuPosition;

    public HomePage()
    {
        Title = "Викторины";
        Content = LoadingView();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            _openMenuQuizId = null;
            _menuPosition = null;
            _quizzes.Clear();
            _quizzes.AddRange((await _repository.GetQuizListAsync())
                .OrderBy(quiz => quiz.Title, StringComparer.CurrentCultureIgnoreCase));

            Build();
        }
        catch (Exception ex)
        {
            Content = MessageView(
                "Не удалось открыть базу данных",
                ex.Message,
                "Повторить",
                async () => await LoadAsync());
        }
    }

    private async Task OpenCreateQuizAsync()
    {
        await Navigation.PushAsync(new QuizEditorPage());
    }

    private void ToggleMenu(int quizId, VisualElement anchor)
    {
        if (_openMenuQuizId == quizId)
        {
            CloseMenu();
            return;
        }

        _openMenuQuizId = quizId;
        _menuPosition = GetMenuPosition(anchor);
        Build();
    }

    private void CloseMenu()
    {
        _openMenuQuizId = null;
        _menuPosition = null;
        Build();
    }

    private async Task EditQuizAsync(int quizId)
    {
        _openMenuQuizId = null;
        _menuPosition = null;
        await Navigation.PushAsync(new QuizEditorPage(quizId));
    }

    private async Task DeleteQuizAsync(QuizListItem quiz)
    {
        _openMenuQuizId = null;
        _menuPosition = null;
        await ConfirmDeleteQuizAsync(quiz);
    }

    private async Task ConfirmDeleteQuizAsync(QuizListItem quiz)
    {
        var confirmed = await DisplayAlertAsync(
            "Удалить викторину?",
            $"Викторина «{quiz.Title}» будет удалена.",
            "Да",
            "Нет");

        if (confirmed)
        {
            await _repository.DeleteQuizAsync(quiz.Id);
            await LoadAsync();
        }
        else
        {
            Build();
        }
    }

    private async Task StartRunAsync(int quizId, QuizRunMode mode)
    {
        var quiz = await _repository.GetQuizAsync(quizId);
        if (quiz is null || quiz.Questions.Count == 0)
        {
            await DisplayAlertAsync("Викторина недоступна", "Викторина не найдена или не содержит вопросов.", "Закрыть");
            await LoadAsync();
            return;
        }

        await Navigation.PushAsync(new QuizRunPage(quiz, mode));
    }
}
