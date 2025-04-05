using System.IO;
using System.Text;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Represents the PDF null object. Section 7.3.8.
/// </summary>
internal sealed class PdfNull : PdfObject
{
    /// <summary>
    /// Singleton instance representing the PDF null value.
    /// </summary>
    public static readonly PdfNull Instance = new PdfNull();

    // Private constructor for singleton pattern
    private PdfNull() { }

    internal override void Write(StreamWriter writer, Encoding encoding)
    {
        writer.Write("null");
    }

    public override string ToString() => "null"; // For debugging
}
