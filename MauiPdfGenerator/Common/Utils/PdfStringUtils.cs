namespace MauiPdfGenerator.Common.Utils;

internal static class PdfStringUtils
{
    public static string NormalizeNewline = "\n";

    public static string NormalizeNewlines(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (!text.Contains('\r'))
        {
            return text;
        }

        return text.Replace("\r\n", NormalizeNewline).Replace("\n\r", NormalizeNewline).Replace("\r", NormalizeNewline);
    }
}