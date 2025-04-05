using System.IO;
using System.Text;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Represents a PDF boolean object (true or false). Section 7.3.2.
/// </summary>
internal sealed class PdfBoolean : PdfObject
{
    public static readonly PdfBoolean True = new PdfBoolean(true);
    public static readonly PdfBoolean False = new PdfBoolean(false);

    public bool Value { get; }

    private PdfBoolean(bool value)
    {
        Value = value;
    }

    internal override void Write(StreamWriter writer, Encoding encoding)
    {
        writer.Write(Value ? "true" : "false");
    }

    public override string ToString() => Value ? "true" : "false"; // For debugging
}
