using MauiPdfGenerator.Common.Utils;
using Xunit;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Common.Utils;

public class PdfStringUtilsTests
{
    [Fact]
    public void NormalizeNewlines_NullOrEmpty_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, PdfStringUtils.NormalizeNewlines(null));
        Assert.Equal(string.Empty, PdfStringUtils.NormalizeNewlines(string.Empty));
    }

    [Fact]
    public void NormalizeNewlines_NoCarriageReturn_ReturnsOriginal()
    {
        const string input = "Hello\nWorld";
        var result = PdfStringUtils.NormalizeNewlines(input);
        Assert.Equal(input, result);
    }

    [Fact]
    public void NormalizeNewlines_CrLf_ReplacedWithLf()
    {
        const string input = "Hello\r\nWorld";
        const string expected = "Hello\nWorld";
        var result = PdfStringUtils.NormalizeNewlines(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeNewlines_Cr_ReplacedWithLf()
    {
        const string input = "Hello\rWorld";
        const string expected = "Hello\nWorld";
        var result = PdfStringUtils.NormalizeNewlines(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeNewlines_LfCr_ReplacedWithLf()
    {
        const string input = "Hello\n\rWorld";
        const string expected = "Hello\nWorld";
        var result = PdfStringUtils.NormalizeNewlines(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeNewlines_MixedNewlines_AllNormalized()
    {
        const string input = "Line1\r\nLine2\n\rLine3\rLine4";
        const string expected = "Line1\nLine2\nLine3\nLine4";
        var result = PdfStringUtils.NormalizeNewlines(input);
        Assert.Equal(expected, result);
    }
}
