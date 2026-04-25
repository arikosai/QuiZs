using QuiZs.Controls;
using QuiZs.Models;

namespace QuiZs.Pages;

public sealed partial class QuizEditorPage
{
    private void Build()
    {
        var root = AppUi.PageGrid();
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowSpacing = 18;

        root.Add(BuildTitleBlock(), 0, 0);
        root.Add(BuildWorkspace(), 0, 1);
        root.Add(BuildFooter(), 0, 2);

        Content = root;
    }

    private View BuildTitleBlock()
    {
        var titleEntry = AppUi.TitleEntry(_title);
        titleEntry.TextChanged += (_, args) => UpdateTitle(args.NewTextValue);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(420))
            },
            ColumnSpacing = 18
        };

        grid.Add(new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                AppUi.Heading("Название викторины", 24),
                titleEntry
            }
        }, 0);

        return AppUi.Card(grid);
    }

    private View BuildWorkspace()
    {
        var workspace = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(280)),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 18
        };

        workspace.Add(BuildQuestionSidebar(), 0);
        workspace.Add(BuildQuestionForm(), 1);

        return workspace;
    }

    private View BuildQuestionSidebar()
    {
        var list = new VerticalStackLayout { Spacing = 8 };

        for (var i = 0; i < _questions.Count; i++)
        {
            var questionIndex = i;
            var isSelected = i == _selectedQuestionIndex;
            var button = new Button
            {
                Text = $"Вопрос {i + 1}: {ShortText(_questions[i].Text, 28)}",
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = isSelected ? AppUi.Accent : AppUi.Surface,
                TextColor = isSelected ? Colors.White : AppUi.TextPrimary,
                BorderColor = AppUi.BorderColor,
                BorderWidth = 1,
                CornerRadius = 8,
                MinimumHeightRequest = 42
            };
            button.Clicked += (_, _) => SelectQuestion(questionIndex);
            list.Add(button);
        }

        var addButton = AppUi.SecondaryButton("Добавить вопрос");
        addButton.Clicked += async (_, _) => await AddQuestionAsync();

        var sidebar = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            RowSpacing = 14
        };
        sidebar.Add(AppUi.Heading("Добавленные вопросы", 18), 0, 0);
        sidebar.Add(new ScrollView { Content = list }, 0, 1);
        sidebar.Add(addButton, 0, 2);

        return AppUi.Card(sidebar);
    }

    private View BuildQuestionForm()
    {
        var question = SelectedQuestion;
        var questionEditor = AppUi.Editor("Текст вопроса", question.Text);
        questionEditor.TextChanged += (_, args) => UpdateQuestionText(args.NewTextValue);

        var answerRows = new VerticalStackLayout { Spacing = 10 };
        for (var i = 0; i < DraftQuestion.AnswerCount; i++)
        {
            answerRows.Add(BuildAnswerRow(question, i));
        }

        return AppUi.Card(new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    AppUi.Heading($"Вопрос {_selectedQuestionIndex + 1}", 24),
                    AppUi.FieldBlock("Текст вопроса", questionEditor),
                    new Label
                    {
                        Text = "Выберите один правильный ответ",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = AppUi.TextPrimary
                    },
                    answerRows
                }
            }
        });
    }

    private View BuildAnswerRow(DraftQuestion question, int answerIndex)
    {
        var radioButton = new RadioButton
        {
            GroupName = $"CorrectAnswer_{_selectedQuestionIndex}",
            IsChecked = question.CorrectAnswerIndex == answerIndex,
            VerticalOptions = LayoutOptions.Center,
            TextColor = AppUi.TextPrimary
        };
        radioButton.CheckedChanged += (_, args) =>
        {
            if (args.Value)
            {
                SetCorrectAnswer(answerIndex);
            }
        };

        var answerEntry = AppUi.Entry($"Вариант ответа {answerIndex + 1}", question.Answers[answerIndex]);
        answerEntry.TextChanged += (_, args) => UpdateAnswerText(answerIndex, args.NewTextValue);

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 8
        };
        row.Add(radioButton, 0);
        row.Add(answerEntry, 1);

        return row;
    }

    private View BuildFooter()
    {
        var cancelButton = AppUi.SecondaryButton("Отмена");
        cancelButton.Clicked += async (_, _) => await CancelAsync();

        var saveButton = AppUi.PrimaryButton("Сохранить викторину");
        saveButton.Clicked += async (_, _) => await SaveAsync();

        var footer = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };
        footer.Add(cancelButton, 1);
        footer.Add(saveButton, 2);

        return footer;
    }

    private static View LoadingView()
    {
        return new Grid
        {
            BackgroundColor = AppUi.PageBackground,
            Children =
            {
                new ActivityIndicator
                {
                    IsRunning = true,
                    Color = AppUi.Accent,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        };
    }

    private static string ShortText(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "без текста";
        }

        var trimmed = text.Trim();
        return trimmed.Length <= maxLength ? trimmed : $"{trimmed[..maxLength]}...";
    }
}
