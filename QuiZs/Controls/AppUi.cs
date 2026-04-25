using Microsoft.Maui.Controls.Shapes;

namespace QuiZs.Controls;

public static class AppUi
{
    public static readonly Color PageBackground = Color.FromArgb("#F5F7FA");
    public static readonly Color Surface = Colors.White;
    public static readonly Color MutedSurface = Color.FromArgb("#EEF3F0");
    public static readonly Color TextPrimary = Colors.Black;
    public static readonly Color TextSecondary = Color.FromArgb("#59645F");
    public static readonly Color Accent = Color.FromArgb("#2D6A4F");
    public static readonly Color AccentDark = Color.FromArgb("#1B4332");
    public static readonly Color BorderColor = Color.FromArgb("#D7DED9");
    public static readonly Color SelectedAnswer = Color.FromArgb("#E7F0EC");
    public static readonly Color Success = Color.FromArgb("#237A3B");
    public static readonly Color SuccessSurface = Color.FromArgb("#DFF3E5");
    public static readonly Color Error = Color.FromArgb("#B42318");
    public static readonly Color ErrorSurface = Color.FromArgb("#FDE1DD");

    public static Grid PageGrid()
    {
        return new Grid
        {
            BackgroundColor = PageBackground,
            Padding = new Thickness(28)
        };
    }

    public static Label Heading(string text, double fontSize)
    {
        return new Label
        {
            Text = text,
            FontSize = fontSize,
            FontAttributes = FontAttributes.Bold,
            TextColor = TextPrimary
        };
    }

    public static Label BodyText(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 15,
            TextColor = TextSecondary
        };
    }

    public static Entry Entry(string placeholder, string text)
    {
        return new Entry
        {
            Text = text,
            Placeholder = placeholder,
            TextColor = TextPrimary,
            PlaceholderColor = TextSecondary,
            BackgroundColor = Surface,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing
        };
    }

    public static Entry TitleEntry(string text)
    {
        return new Entry
        {
            Text = text,
            TextColor = TextPrimary,
            PlaceholderColor = TextSecondary,
            BackgroundColor = Surface,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            MinimumHeightRequest = 52
        };
    }

    public static Editor Editor(string placeholder, string text)
    {
        return new Editor
        {
            Text = text,
            Placeholder = placeholder,
            TextColor = TextPrimary,
            PlaceholderColor = TextSecondary,
            BackgroundColor = Surface,
            AutoSize = EditorAutoSizeOption.TextChanges,
            MinimumHeightRequest = 110
        };
    }

    public static View FieldBlock(string label, View input)
    {
        return new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                new Label
                {
                    Text = label,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = TextPrimary
                },
                input
            }
        };
    }

    public static Border Card(View content)
    {
        return new Border
        {
            Content = content,
            Padding = new Thickness(16),
            BackgroundColor = Surface,
            Stroke = BorderColor,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 }
        };
    }

    public static Button PrimaryButton(string text)
    {
        return new Button
        {
            Text = text,
            BackgroundColor = Accent,
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(14, 10),
            MinimumHeightRequest = 44
        };
    }

    public static Button SecondaryButton(string text)
    {
        return new Button
        {
            Text = text,
            BackgroundColor = Surface,
            TextColor = AccentDark,
            BorderColor = Accent,
            BorderWidth = 1,
            CornerRadius = 8,
            Padding = new Thickness(14, 10),
            MinimumHeightRequest = 44
        };
    }

    public static Button IconButton(string text)
    {
        return new Button
        {
            Text = text,
            BackgroundColor = Surface,
            TextColor = TextPrimary,
            BorderColor = BorderColor,
            BorderWidth = 1,
            CornerRadius = 8,
            FontAttributes = FontAttributes.Bold,
            MinimumHeightRequest = 44,
            WidthRequest = 48,
            Padding = new Thickness(0)
        };
    }

    public static Button MenuButton(string text)
    {
        return new Button
        {
            Text = text,
            BackgroundColor = Surface,
            TextColor = TextPrimary,
            HorizontalOptions = LayoutOptions.Fill,
            CornerRadius = 8,
            Padding = new Thickness(12, 8),
            MinimumHeightRequest = 38
        };
    }

    public static View ResultLine(string label, string value, Color textColor, Color backgroundColor)
    {
        return new Border
        {
            BackgroundColor = backgroundColor,
            Stroke = BorderColor,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(12, 8),
            Content = new VerticalStackLayout
            {
                Spacing = 2,
                Children =
                {
                    new Label
                    {
                        Text = label,
                        FontSize = 12,
                        TextColor = TextSecondary
                    },
                    new Label
                    {
                        Text = value,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = textColor
                    }
                }
            }
        };
    }
}
