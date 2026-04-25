namespace QuiZs.Extensions;

public static class StringExtensions
{
    extension(string value)
    {
        public string NormalizeText() => value.Trim();

        public string NormalizeRequiredText(string paramName)
        {
            var normalizedValue = value.NormalizeText();

            return normalizedValue.Length == 0
                ? throw new ArgumentException("Value cannot be empty or whitespace.", paramName)
                : normalizedValue;
        }
    }
}
