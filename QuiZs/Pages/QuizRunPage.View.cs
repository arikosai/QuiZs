using QuiZs.Controls;
using QuiZs.Models;

namespace QuiZs.Pages;

public sealed partial class QuizRunPage
{
    private void Build()
    {
        var root = AppUi.PageGrid();
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowSpacing = 16;

        root.Add(BuildHeader(), 0, 0);
        root.Add(BuildQuestionCard(), 0, 1);
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
                AppUi.Heading(_quiz.Title, 26),
                AppUi.BodyText($"Вопрос {_currentQuestionIndex + 1} из {_quiz.Questions.Count}")
            }
        };
    }

    private View BuildQuestionCard()
    {
        var question = _quiz.Questions[_currentQuestionIndex];
        var selectedAnswer = _mode == QuizRunMode.Pass ? _selectedAnswers[_currentQuestionIndex] : null;
        var alreadyAnswered = selectedAnswer is not null;
        var answers = new VerticalStackLayout { Spacing = 10 };

        for (var i = 0; i < question.Answers.Count; i++)
        {
            answers.Add(BuildAnswerButton(question.Answers[i], i, selectedAnswer, alreadyAnswered));
        }

        var hint = _mode == QuizRunMode.Pass
            ? "Ответ фиксируется после первого выбора. Правильные ответы будут показаны после завершения."
            : "Используйте кнопки навигации для просмотра вопросов.";

        return AppUi.Card(new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 18,
                Children =
                {
                    AppUi.Heading(question.Text, 22),
                    AppUi.BodyText(hint),
                    answers
                }
            }
        });
    }

    private View BuildAnswerButton(Answer answer, int answerIndex, int? selectedAnswer, bool alreadyAnswered)
    {
        var isSelected = selectedAnswer == answerIndex;
        var button = new Button
        {
            Text = answer.Text,
            HorizontalOptions = LayoutOptions.Fill,
            BackgroundColor = isSelected ? AppUi.SelectedAnswer : AppUi.Surface,
            TextColor = AppUi.TextPrimary,
            BorderColor = isSelected ? AppUi.Accent : AppUi.BorderColor,
            BorderWidth = isSelected ? 2 : 1,
            CornerRadius = 8,
            MinimumHeightRequest = 48
        };

        if (_mode == QuizRunMode.Pass && !alreadyAnswered)
        {
            button.Clicked += (_, _) => SelectAnswer(answerIndex);
        }
        else
        {
            button.InputTransparent = true;
        }

        return button;
    }

    private View BuildFooter()
    {
        var previousButton = AppUi.SecondaryButton("Предыдущий вопрос");
        previousButton.IsEnabled = _currentQuestionIndex > 0;
        previousButton.Clicked += (_, _) => MovePrevious();

        var isLastQuestion = _currentQuestionIndex == _quiz.Questions.Count - 1;
        var nextButton = AppUi.PrimaryButton(isLastQuestion
            ? (_mode == QuizRunMode.Demo ? "Завершить демонстрацию" : "Завершить викторину")
            : "Следующий вопрос");
        nextButton.Clicked += async (_, _) => await MoveNextAsync();

        var homeButton = AppUi.SecondaryButton("На главную");
        homeButton.Clicked += async (_, _) => await GoHomeAsync();

        var footer = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };
        footer.Add(previousButton, 0);
        footer.Add(homeButton, 2);
        footer.Add(nextButton, 3);

        return footer;
    }
}
