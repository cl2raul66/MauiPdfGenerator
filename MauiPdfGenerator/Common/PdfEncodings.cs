using System.Text;
using System.Globalization;

namespace MauiPdfGenerator.Common;

/// <summary>
/// Provides helper methods for encoding and escaping text according to PDF specificaitons.
/// Internal as it's an implementation detail.
/// </summary>
internal static class PdfEncodings
{
    // Use ASCII for structure, comments, keywords. Content streams often use it too.
    public static readonly Encoding StructureEncoding = Encoding.ASCII;

    // PDFDocEncoding can represent many common characters used in metadata.
    // For stream content with non-ASCII, using UTF-8 with appropriate font setup (or UTF-16BE with BOM) is better.
    // We'll stick to simpler encoding for now.

    /// <summary>
    /// Escapes a string for use as a PDF Literal String '(...)'
    /// Handles balanced parentheses, backslashes, and non-printable ASCII.
    /// Does *not* handle non-ASCII characters properly for standard fonts without correct encoding setup.
    /// </summary>
    public static string EscapePdfLiteralString(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var sb = new StringBuilder();
        foreach (char c in text)
        {
            switch (c)
            {
                case '(': sb.Append(@"\("); break;
                case ')': sb.Append(@"\)"); break;
                case '\\': sb.Append(@"\\"); break;
                case '\n': sb.Append(@"\n"); break;
                case '\r': sb.Append(@"\r"); break;
                case '\t': sb.Append(@"\t"); break;
                case '\b': sb.Append(@"\b"); break;
                case '\f': sb.Append(@"\f"); break;
                default:
                    // Handle non-printable ASCII using octal escape \ddd
                    if (c < 32 || c > 126)
                    {
                        // Basic handling: Replace non-PDFDocEncoding compatible chars or higher chars
                        // A full implementation would need mapping based on the target encoding.
                        // For now, replace with octal if simple, else '?'.
                        if (c < 256) // Only escape if potentially representable in single byte
                        {
                            sb.Append('\\').Append(Convert.ToString(c, 8).PadLeft(3, '0'));
                        }
                        else
                        {
                            sb.Append('?'); // Placeholder for non-encodable chars
                        }
                    }
                    else
                    {
                        sb.Append(c); // Printable ASCII
                    }
                    break;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Escapes a string for use as a PDF Hexadecimal String "&lt;...>"
    /// </summary>
    public static string EscapePdfHexString(byte[] data)
    {
        if (data is null || data.Length == 0) return string.Empty;
        // Each byte becomes two hex characters.
        return Convert.ToHexString(data);
    }

    /// <summary>
    /// Encodes a string into bytes suitable for placing directly into a content stream
    /// when using standard fonts (like Helvetica) which assume WinAnsiEncoding or similar.
    /// Replaces unsupported characters.
    /// </summary>
    public static byte[] EncodeForContentStream(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        // A simple approximation using ASCII + replace for > 127
        // Proper solution involves mapping to WinAnsiEncoding or using CIDFonts for Unicode.
        var bytes = new List<byte>();
        foreach (char c in text)
        {
            if (c < 128)
            {
                bytes.Add((byte)c);
            }
            else
            {
                // TODO: Implement proper WinAnsiEncoding mapping or use Unicode approach
                bytes.Add((byte)'?'); // Placeholder
            }
        }
        return [.. bytes];
    }

    /// <summary>
    /// Formats a float according to PDF conventions (using '.' as decimal separator).
    /// </summary>
    public static string FormatFloat(float value)
    {
        // Use "R" format specifier for round-trippable precision if needed,
        // or specify decimal places e.g., "F3"
        return value.ToString("0.###", CultureInfo.InvariantCulture); // Limit decimal places
    }
}
