using MauiPdfGenerator.Core.Fonts;
using MauiPdfGenerator.Core.Images;
using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Structure;
using System.Globalization;
using System.Text;

namespace MauiPdfGenerator.Core.Content;

/// <summary>
/// Represents a PDF content stream, used to define the graphical content of a page.
/// Provides methods to append PDF content stream operators. Inherits from PdfStream.
/// Section 7.8.
/// </summary>
internal class PdfContentStream : PdfStream
{
    private readonly PdfDocument _document;
    private readonly PdfResources _resources;
    private readonly MemoryStream _contentBytes = new();
    private readonly StreamWriter _writer; // Writes text representation of operators
    private readonly Encoding _pdfEncoding = Encoding.ASCII; // Operators are typically ASCII

    private PdfGraphicsState CurrentGraphicsState { get; } = new PdfGraphicsState(); // TODO: Use this for optimization

    public PdfContentStream(PdfDocument document, PdfResources resources) : base()
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        // Use UTF8 without BOM for the writer as it handles ASCII well and is common
        _writer = new StreamWriter(_contentBytes, new UTF8Encoding(false), leaveOpen: true);
        // Compression is good for content streams
        Dictionary.Add(PdfName.Filter, PdfName.FlateDecode);
    }

    // --- Content Stream Operator Methods (High Level Abstractions) ---

    public void SaveGraphicsState() => AppendOperator("q");
    public void RestoreGraphicsState() => AppendOperator("Q");
    public void SetLineWidth(double width) => AppendOperator($"{FormatDouble(width)} w");
    // Add SetLineCap, SetLineJoin, SetMiterLimit, SetDashPattern...

    // TODO: Implement SetStrokeColor, SetFillColor using PdfColorState helper

    // Path construction
    public void MoveTo(double x, double y) => AppendOperator($"{FormatDouble(x)} {FormatDouble(y)} m");
    public void LineTo(double x, double y) => AppendOperator($"{FormatDouble(x)} {FormatDouble(y)} l");
    public void AppendRectangle(double x, double y, double width, double height) => AppendOperator($"{FormatDouble(x)} {FormatDouble(y)} {FormatDouble(width)} {FormatDouble(height)} re");
    public void ClosePath() => AppendOperator("h");

    // Path painting
    public void StrokePath() => AppendOperator("S");
    public void FillPath() => AppendOperator("f");
    public void FillAndStrokePath() => AppendOperator("B");
    public void FillPathEvenOdd() => AppendOperator("f*");
    public void FillAndStrokePathEvenOdd() => AppendOperator("B*");
    public void EndPath() => AppendOperator("n"); // No-op path painting

    // Clipping paths
    public void ClipPathWinding() => AppendOperator("W");
    public void ClipPathEvenOdd() => AppendOperator("W*");


    // Text Objects
    public void BeginText() => AppendOperator("BT");
    public void EndText() => AppendOperator("ET");

    // Text State
    public void SetFont(PdfFontBase font, double size)
    {
        // TODO: Check if font/size actually changed using PdfGraphicsState
        PdfName fontName = _resources.GetResourceName(font);
        AppendOperator($"{fontName.Value} {FormatDouble(size)} Tf");
        CurrentGraphicsState.CurrentFont = font; // Update state
        CurrentGraphicsState.CurrentFontSize = size;
    }
    public void SetTextLeading(double leading) => AppendOperator($"{FormatDouble(leading)} TL");
    public void SetCharacterSpacing(double spacing) => AppendOperator($"{FormatDouble(spacing)} Tc");
    public void SetWordSpacing(double spacing) => AppendOperator($"{FormatDouble(spacing)} Tw");
    public void SetHorizontalScaling(double scalePercent) => AppendOperator($"{FormatDouble(scalePercent)} Tz");
    // Add SetTextRenderMode (Tr)

    // Text Positioning
    public void SetTextMatrix(double a, double b, double c, double d, double e, double f) => AppendOperator($"{Fd(a)} {Fd(b)} {Fd(c)} {Fd(d)} {Fd(e)} {Fd(f)} Tm");
    public void MoveToNextLine() => AppendOperator("T*"); // Move to start of next line (using current leading)
    public void MoveTextPosition(double tx, double ty) => AppendOperator($"{Fd(tx)} {Fd(ty)} Td"); // Offset from start of current line
    public void MoveToNextLineWithLeading(double tx, double ty) => AppendOperator($"{Fd(tx)} {Fd(ty)} TD"); // Set leading AND move

    // Text Showing
    public void ShowText(string text, PdfFontBase? fontForEncoding = null) // Font needed for correct encoding
    {
        // TODO: Handle encoding correctly based on the font.
        // This is complex. For simple fonts/encodings, might map to bytes.
        // For CIDFonts, uses hex strings representing CIDs/GIDs.
        // For now, use a placeholder assuming simple encoding via PdfString
        var pdfString = new PdfString(text, useUtf16: false); // Simplification! Needs font encoding logic.
        AppendStringLiteral(pdfString); // Write the literal string
        AppendOperator("Tj");
    }
    // Add ShowTextWithGlyphPositioning ('TJ' operator with arrays/numbers)


    // Images / XObjects
    public void DrawXObject(PdfImageXObject image) // Or potentially other XObject types later
    {
        // TODO: Need to manage Current Transformation Matrix (CTM)
        // Typically: q (save state) -> Transform CTM (cm operator) -> Draw -> Q (restore state)
        PdfName imageName = _resources.GetResourceName(image);
        // Example: Draw image at 1x1 size at current origin
        // AppendOperator("1 0 0 1 0 0 cm"); // Optional transform
        AppendOperator($"{imageName.Value} Do");
    }


    // --- Low Level Append Methods ---

    /// <summary>
    /// Appends a raw PDF operator string to the content stream.
    /// Automatically adds a newline character after the operator.
    /// </summary>
    /// <param name="op">The PDF operator and its operands (e.g., "10 20 Td", "Q").</param>
    public void AppendOperator(string op)
    {
        _writer.Write(op);
        _writer.Write('\n'); // Standard PDF newline after operators
    }

    /// <summary>
    /// Appends a formatted string literal, handling escaping.
    /// </summary>
    public void AppendStringLiteral(PdfString str)
    {
        // PdfString handles its own formatting/escaping when written
        str.Write(_writer, _pdfEncoding);
    }


    // --- Finalization ---

    /// <summary>
    /// Finishes writing to the internal buffer and returns the complete content bytes.
    /// Should be called before the PdfContentStream is written as part of an indirect object.
    /// </summary>
    internal byte[] GetContentBytes()
    {
        _writer.Flush(); // Ensure all buffered text is written to the MemoryStream
        UnfilteredData = _contentBytes.ToArray(); // Update the base class data
        return UnfilteredData; // Return the raw (unfiltered) bytes
    }

    protected override byte[] ApplyFilters()
    {
        GetContentBytes(); // Ensure the writer is flushed and UnfilteredData is up-to-date
        return base.ApplyFilters(); // Apply compression etc.
    }

    // --- Helpers ---
    private static string FormatDouble(double value)
    {
        // Use InvariantCulture and a reasonable format to avoid excessive precision / scientific notation
        // PDF spec doesn't define precision, but 3-5 decimal places is often sufficient.
        // "G" format with InvariantCulture often works well. Adjust if needed.
        return value.ToString("0.####", CultureInfo.InvariantCulture); // Example: up to 4 decimal places
    }
    private static string Fd(double value) => FormatDouble(value); // Shorter alias


    // Dispose the writer when the stream is disposed
    // (PdfStream base class doesn't currently implement IDisposable)
    public void Dispose()
    {
        _writer.Dispose();
        _contentBytes.Dispose();
        // Consider adding IDisposable to PdfStream and calling base.Dispose() if needed
    }
}
