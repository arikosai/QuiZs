using Microsoft.Maui.Controls.Shapes;
using QuiZs.Controls;
using QuiZs.Models;

namespace QuiZs.Pages;

public sealed partial class HomePage
{
    private void Build()
    {
        var root = AppUi.PageGrid();
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        root.RowSpacing = 22;

        root.Add(BuildHeader(), 0, 0);
        root.Add(BuildQuizList(), 0, 1);

        if (_openMenuQuizId is not null)
        {
            var floatingMenu = BuildFloatingMenu(_openMenuQuizId.Value);
            root.Add(floatingMenu, 0, 0);
            root.SetRowSpan(floatingMenu, 2);
        }

        Content = root;
    }

    private View BuildHeader()
    {
        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 16
        };

        header.Add(new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                AppUi.Heading("Викторины", 32),
                AppUi.BodyText("Выберите режим или создайте новый квиз.")
            }
        }, 0);

        var createButton = AppUi.PrimaryButton("Создать викторину");
        createButton.Clicked += async (_, _) => await OpenCreateQuizAsync();
        header.Add(createButton, 1);

        return header;
    }

    private View BuildQuizList()
    {
        var list = new VerticalStackLayout { Spacing = 12 };

        if (_quizzes.Count == 0)
        {
            list.Add(AppUi.Card(new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    AppUi.Heading("Список викторин пуст", 20),
                    AppUi.BodyText("Нажмите «Создать викторину», добавьте минимум два вопроса и сохраните.")
                }
            }));
        }
        else
        {
            foreach (var quiz in _quizzes)
            {
                list.Add(BuildQuizRow(quiz));
            }
        }

        return new ScrollView { Content = list };
    }

    private View BuildQuizRow(QuizListItem quiz)
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10,
            Padding = new Thickness(4)
        };

        row.Add(new VerticalStackLayout
        {
            Spacing = 3,
            Children =
            {
                AppUi.Heading(quiz.Title, 20),
                AppUi.BodyText($"Вопросов: {quiz.QuestionCount}")
            }
        }, 0);

        var demoButton = AppUi.SecondaryButton("Демонстрация");
        demoButton.Clicked += async (_, _) => await StartRunAsync(quiz.Id, QuizRunMode.Demo);
        row.Add(demoButton, 1);

        var passButton = AppUi.PrimaryButton("Прохождение");
        passButton.Clicked += async (_, _) => await StartRunAsync(quiz.Id, QuizRunMode.Pass);
        row.Add(passButton, 2);

        var menuButton = AppUi.IconButton("...");
        menuButton.Clicked += (_, _) => ToggleMenu(quiz.Id, menuButton);
        row.Add(menuButton, 3);

        return AppUi.Card(row);
    }

    private View BuildFloatingMenu(int quizId)
    {
        var quiz = _quizzes.FirstOrDefault(item => item.Id == quizId);
        if (quiz is null)
        {
            return new Grid();
        }

        var editButton = AppUi.MenuButton("Редактировать");
        editButton.Clicked += async (_, _) => await EditQuizAsync(quiz.Id);

        var deleteButton = AppUi.MenuButton("Удалить");
        deleteButton.TextColor = AppUi.Error;
        deleteButton.Clicked += async (_, _) => await DeleteQuizAsync(quiz);

        var closeArea = new BoxView
        {
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        closeArea.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(CloseMenu)
        });

        var overlay = new Grid
        {
            BackgroundColor = Colors.Transparent,
            Children =
            {
                closeArea,
                new Border
                {
                    BackgroundColor = AppUi.Surface,
                    Stroke = AppUi.BorderColor,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    Padding = new Thickness(8),
                    WidthRequest = 260,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(_menuPosition?.X ?? 0, _menuPosition?.Y ?? 0, 0, 0),
                    Content = new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children = { editButton, deleteButton }
                    }
                }
            }
        };

        return overlay;
    }

    private Point GetMenuPosition(VisualElement anchor)
    {
        const double menuWidth = 260;
        const double gap = 6;

        var anchorPosition = GetPositionInRoot(anchor);
        var contentWidth = Math.Max(0, Width - 56);
        var x = anchorPosition.X + anchor.Width - menuWidth;
        var y = anchorPosition.Y + anchor.Height + gap;

        x = Math.Max(0, x);
        if (contentWidth > 0)
        {
            x = Math.Min(x, Math.Max(0, contentWidth - menuWidth));
        }

        return new Point(x, Math.Max(0, y));
    }

    private Point GetPositionInRoot(VisualElement element)
    {
        var x = 0d;
        var y = 0d;
        Element? current = element;

        while (current is VisualElement visual && current != Content)
        {
            x += visual.X;
            y += visual.Y;

            if (visual.Parent is ScrollView scrollView)
            {
                y -= scrollView.ScrollY;
            }

            current = visual.Parent;
        }

        if (Content is Layout root)
        {
            x -= root.Padding.Left;
            y -= root.Padding.Top;
        }

        return new Point(x, y);
    }

    private static View LoadingView()
    {
        return new Grid
        {
            BackgroundColor = AppUi.PageBackground,
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 12,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new ActivityIndicator
                        {
                            IsRunning = true,
                            Color = AppUi.Accent
                        },
                        AppUi.BodyText("Загрузка викторин...")
                    }
                }
            }
        };
    }

    private static View MessageView(string title, string text, string buttonText, Func<Task> action)
    {
        var button = AppUi.PrimaryButton(buttonText);
        button.Clicked += async (_, _) => await action();

        return new Grid
        {
            BackgroundColor = AppUi.PageBackground,
            Padding = new Thickness(24),
            Children =
            {
                AppUi.Card(new VerticalStackLayout
                {
                    Spacing = 14,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        AppUi.Heading(title, 22),
                        AppUi.BodyText(text),
                        button
                    }
                })
            }
        };
    }
}
