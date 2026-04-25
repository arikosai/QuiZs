using QuiZs.Controls;

namespace QuiZs.Pages;

public sealed partial class QuizResultsPage
{
    private void Build()
    {
        var root = AppUi.PageGrid();
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowSpacing = 18;

        root.Add(BuildHeader(), 0, 0);
        root.Add(BuildResultList(), 0, 1);
        root.Add(BuildFooter(), 0, 2);

        Content = root;
    }

    private View BuildHeader()
    {
        return new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                AppUi.Heading("Результаты", 28),
                AppUi.Heading($"Правильных ответов: {CountCorrectAnswers()} из {_quiz.Questions.Count}", 24)
            }
        };
    }

    private View BuildResultList()
    {
        var list = new VerticalStackLayout { Spacing = 12 };
        for (var i = 0; i < _quiz.Questions.Count; i++)
        {
            list.Add(BuildQuestionResult(i));
        }

        return new ScrollView { Content = list };
    }

    private View BuildQuestionResult(int questionIndex)
    {
        var question = _quiz.Questions[questionIndex];
        var selectedAnswerIndex = _selectedAnswers[questionIndex];
        var correctAnswer = question.Answers.First(answer => answer.IsCorrect);
        var selectedAnswer = selectedAnswerIndex is null ? null : question.Answers[selectedAnswerIndex.Value];
        var isCorrect = selectedAnswer?.IsCorrect == true;

        var selectedText = selectedAnswer is null ? "Ответ не выбран" : selectedAnswer.Text;
        var selectedColor = selectedAnswer is null ? AppUi.TextSecondary : isCorrect ? AppUi.Success : AppUi.Error;
        var selectedBackground = selectedAnswer is null
            ? AppUi.MutedSurface
            : isCorrect ? AppUi.SuccessSurface : AppUi.ErrorSurface;

        return AppUi.Card(new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                AppUi.Heading($"Вопрос {questionIndex + 1}. {question.Text}", 18),
                AppUi.ResultLine("Ваш ответ", selectedText, selectedColor, selectedBackground),
                AppUi.ResultLine("Правильный ответ", correctAnswer.Text, AppUi.Success, AppUi.SuccessSurface)
            }
        });
    }

    private View BuildFooter()
    {
        var homeButton = AppUi.PrimaryButton("На главную");
        homeButton.Clicked += async (_, _) => await GoHomeAsync();

        return new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.End,
            Children = { homeButton }
        };
    }
}
