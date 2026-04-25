using QuiZs.Models;

namespace QuiZs.Pages;

public sealed partial class QuizRunPage : ContentPage
{
    private readonly Quiz _quiz;
    private readonly QuizRunMode _mode;
    private readonly int?[] _selectedAnswers;

    private int _currentQuestionIndex;

    public QuizRunPage(Quiz quiz, QuizRunMode mode)
    {
        _quiz = quiz;
        _mode = mode;
        _selectedAnswers = new int?[quiz.Questions.Count];
        Title = mode == QuizRunMode.Demo ? "Демонстрация" : "Прохождение";

        Build();
    }

    private void SelectAnswer(int answerIndex)
    {
        _selectedAnswers[_currentQuestionIndex] = answerIndex;
        Build();
    }

    private void MovePrevious()
    {
        if (_currentQuestionIndex == 0)
        {
            return;
        }

        _currentQuestionIndex--;
        Build();
    }

    private async Task MoveNextAsync()
    {
        var isLastQuestion = _currentQuestionIndex == _quiz.Questions.Count - 1;
        if (isLastQuestion)
        {
            if (_mode == QuizRunMode.Demo)
            {
                await Navigation.PopToRootAsync();
            }
            else
            {
                await Navigation.PushAsync(new QuizResultsPage(_quiz, _selectedAnswers));
            }

            return;
        }

        _currentQuestionIndex++;
        Build();
    }

    private async Task GoHomeAsync()
    {
        await Navigation.PopToRootAsync();
    }
}
