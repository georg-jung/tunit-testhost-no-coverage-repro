namespace Contoso.Core;

public static class StringUtility
{
    public static string Reverse(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var chars = input.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    public static bool IsPalindrome(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var reversed = Reverse(input);
        return string.Equals(input, reversed, StringComparison.OrdinalIgnoreCase);
    }

    public static string TruncateWithEllipsis(string input, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (maxLength < 3)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be at least 3.");

        if (input.Length <= maxLength)
            return input;

        return string.Concat(input.AsSpan(0, maxLength - 3), "...");
    }
}
