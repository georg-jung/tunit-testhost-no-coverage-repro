using Contoso.Core;

namespace Contoso.Tests;

public class StringUtilityTests
{
    [Test]
    public async Task Reverse_ReturnsReversedString()
    {
        var result = StringUtility.Reverse("hello");

        await Assert.That(result).IsEqualTo("olleh");
    }

    [Test]
    public async Task Reverse_EmptyString_ReturnsEmpty()
    {
        var result = StringUtility.Reverse("");

        await Assert.That(result).IsEqualTo("");
    }

    [Test]
    public async Task IsPalindrome_ReturnsTrueForPalindrome()
    {
        var result = StringUtility.IsPalindrome("racecar");

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsPalindrome_ReturnsFalseForNonPalindrome()
    {
        var result = StringUtility.IsPalindrome("hello");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task TruncateWithEllipsis_ShortString_ReturnsOriginal()
    {
        var result = StringUtility.TruncateWithEllipsis("hi", 10);

        await Assert.That(result).IsEqualTo("hi");
    }

    [Test]
    public async Task TruncateWithEllipsis_LongString_Truncates()
    {
        var result = StringUtility.TruncateWithEllipsis("hello world", 8);

        await Assert.That(result).IsEqualTo("hello...");
    }
}
