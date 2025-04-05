using System.Text;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Represents a PDF string object (literal or hexadecimal). Section 7.3.4.
/// Strings represent sequences of bytes.
/// </summary>
internal sealed class PdfString : PdfObject
{
    // Store the raw bytes. Text interpretation depends on context (encoding, font).
    private readonly byte[] _bytes;
    private readonly bool _forceHexFormat;

    /// <summary>
    /// Initializes a new instance of PdfString from raw bytes.
    /// </summary>
    /// <param name="bytes">The raw bytes of the string.</param>
    /// <param name="forceHexFormat">If true, always write using hexadecimal format &lt;...>.</param>
    public PdfString(byte[] bytes, bool forceHexFormat = false)
    {
        _bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        _forceHexFormat = forceHexFormat;
    }

    /// <summary>
    /// Initializes a new instance of PdfString from a C# string,
    /// encoding it using PDFDocEncoding (suitable for many metadata fields and simple text)
    /// or UTF16 Big Endian with BOM for broader character support.
    /// </summary>
    /// <param name="value">The C# string value.</param>
    /// <param name="useUtf16">If true, encode using UTF-16BE with BOM; otherwise use PDFDocEncoding.</param>
    /// <param name="forceHexFormat">If true, always write using hexadecimal format &lt;...>.</param>
    public PdfString(string value, bool useUtf16 = false, bool forceHexFormat = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        _forceHexFormat = forceHexFormat;

        if (useUtf16)
        {
            // Prepend BOM (Byte Order Mark) for UTF-16BE
            var utf16Bytes = Encoding.BigEndianUnicode.GetBytes(value);
            _bytes = new byte[utf16Bytes.Length + 2];
            _bytes[0] = 0xFE; // BOM U+FEFF
            _bytes[1] = 0xFF;
            Buffer.BlockCopy(utf16Bytes, 0, _bytes, 2, utf16Bytes.Length);
        }
        else
        {
            // Attempt PDFDocEncoding (requires a custom implementation or careful mapping)
            // For simplicity here, we might default to Latin1 (ISO-8859-1) which covers
            // a subset of PDFDocEncoding and standard ASCII. A full PDFDocEncoding is better.
            _bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(value); // Simplification!
                                                                         // TODO: Implement proper PDFDocEncoding mapping for robustness.
        }
    }


    internal override void Write(StreamWriter writer, Encoding encoding) // Encoding param is less relevant here as we work with bytes
    {
        bool useHex = _forceHexFormat || ShouldUseHexFormat(_bytes);

        if (useHex)
        {
            WriteHexadecimal(writer, _bytes);
        }
        else
        {
            WriteLiteral(writer, _bytes);
        }
    }

    /// <summary>
    /// Determines if the hexadecimal format should be preferred for writing.
    /// Hex is often better for arbitrary binary data or strings with many non-printable chars.
    /// </summary>
    private bool ShouldUseHexFormat(byte[] bytes)
    {
        // Heuristic: If it contains non-printable ASCII or characters likely
        // problematic in literal strings, prefer hex.
        // This is a simple heuristic, can be refined.
        foreach (byte b in bytes)
        {
            if (b < 32 && b != '\n' && b != '\r' && b != '\t' && b != '\f') return true; // Control chars other than common whitespace
            if (b == '(' || b == ')' || b == '\\') return true; // Chars requiring escape in literal
            if (b > 126) return true; // Non-ASCII often better represented in hex
        }
        return false;
    }


    /// <summary>
    /// Writes the string in PDF Literal String Format (...).
    /// </summary>
    private static void WriteLiteral(StreamWriter writer, byte[] bytes)
    {
        writer.Write('(');

        int balancedParens = 0; // Track parenthesis balance for potential escaping issues

        foreach (byte b in bytes)
        {
            char c = (char)b; // Treat byte as simple char for escaping logic

            switch (c)
            {
                case '(':
                    writer.Write(@"\(");
                    balancedParens++;
                    break;
                case ')':
                    writer.Write(@"\)");
                    balancedParens--;
                    break;
                case '\\':
                    writer.Write(@"\\");
                    break;
                // Standard C-like escapes for common whitespace
                case '\n': writer.Write(@"\n"); break;
                case '\r': writer.Write(@"\r"); break;
                case '\t': writer.Write(@"\t"); break;
                case '\b': writer.Write(@"\b"); break; // Backspace (less common)
                case '\f': writer.Write(@"\f"); break; // Form feed (less common)
                default:
                    // Write printable ASCII characters directly
                    if (b >= 32 && b <= 126)
                    {
                        writer.Write(c);
                    }
                    else
                    {
                        // Write non-printable or extended characters as octal escapes (\ddd)
                        writer.Write($@"\{b:D3}"); // Pad with leading zeros if needed
                    }
                    break;
            }
            // PDF spec is vague on deeply nested unbalanced parens, but some readers might fail.
            // This check is optional, Hex format avoids this complexity.
            // if(balancedParens < -10) throw new PdfFormatException("Deeply nested unbalanced parentheses in literal string.");
        }
        writer.Write(')');
    }


    /// <summary>
    /// Writes the string in PDF Hexadecimal String Format &lt;...>.
    /// </summary>
    private static void WriteHexadecimal(StreamWriter writer, byte[] bytes)
    {
        writer.Write('<');
        foreach (byte b in bytes)
        {
            writer.Write(b.ToString("X2")); // Ensure two hex digits per byte
        }
        // PDF Spec 7.3.4.3: "An odd number of hexadecimal digits is not recommended."
        // We always write pairs, so this is fine. Add a '0' if needed? Usually not necessary.
        writer.Write('>');
    }


    public override string ToString() // For debugging - tries to decode simply
    {
        try
        {
            // Attempt simple decoding for debug view, might not match PDF interpretation
            if (_bytes.Length >= 2 && _bytes[0] == 0xFE && _bytes[1] == 0xFF)
                return Encoding.BigEndianUnicode.GetString(_bytes, 2, _bytes.Length - 2);
            else
                return Encoding.GetEncoding("ISO-8859-1").GetString(_bytes); // Simplification!
        }
        catch { return "<Binary Data>"; }
    }
}
