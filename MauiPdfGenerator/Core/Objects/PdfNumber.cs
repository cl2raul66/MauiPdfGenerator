using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Represents a PDF numeric object (integer or real). Section 7.3.3.
/// </summary>
internal sealed class PdfNumber : PdfObject
{
    public double Value { get; }
    public bool IsInteger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfNumber"/> class with an integer value.
    /// </summary>
    public PdfNumber(long value)
    {
        Value = value;
        IsInteger = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfNumber"/> class with a real value.
    /// </summary>
    public PdfNumber(double value)
    {
        Value = value;
        IsInteger = false; // Treat explicitly passed double as real
    }

    internal override void Write(StreamWriter writer, Encoding encoding)
    {
        // PDF specification requires using '.' as decimal separator, regardless of culture.
        // Use InvariantCulture for formatting.
        if (IsInteger)
        {
            writer.Write(((long)Value).ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            // Format double precisely, avoiding scientific notation where possible,
            // and ensuring '.' is the decimal separator.
            // Use "R" (round-trip) format specifier for precision, then ensure compatibility.
            string formatted = Value.ToString("R", CultureInfo.InvariantCulture);
            // Basic check to avoid scientific notation if it occurred (might need refinement)
            if (formatted.Contains('E') || formatted.Contains('e'))
            {
                // Fallback to a fixed-point representation with reasonable precision
                // Adjust precision as needed (e.g., 6 decimal places)
                formatted = Value.ToString("0.######", CultureInfo.InvariantCulture);
            }
            writer.Write(formatted);
        }
    }

    public override string ToString() // For debugging
    {
        if (IsInteger) return ((long)Value).ToString(CultureInfo.InvariantCulture);
        // Simple version for debugging, Write method is the source of truth for PDF output
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}
