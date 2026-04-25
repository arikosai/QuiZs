using QuiZs.Models;

namespace QuiZs.Pages;

public sealed partial class QuizResultsPage : ContentPage
{
    private readonly Quiz _quiz;
    private readonly int?[] _selectedAnswers;

    public QuizResultsPage(Quiz quiz, int?[] selectedAnswers)
    {
        _quiz = quiz;
        _selectedAnswers = selectedAnswers.ToArray();
        Title = "Результаты";

        Build();
    }

    private async Task GoHomeAsync()
    {
        await Navigation.PopToRootAsync();
    }

    private int CountCorrectAnswers()
    {
        var correctCount = 0;

        for (var i = 0; i < _quiz.Questions.Count; i++)
        {
            var selectedAnswer = _selectedAnswers[i];
            if (selectedAnswer >= 0 &&
                selectedAnswer.Value < _quiz.Questions[i].Answers.Count &&
                _quiz.Questions[i].Answers[selectedAnswer.Value].IsCorrect)
            {
                correctCount++;
            }
        }

        return correctCount;
    }
}
