using System.Text;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Represents a PDF name object (e.g., /Type, /Page). Section 7.3.5.
/// Names are case-sensitive.
/// </summary>
internal sealed class PdfName : PdfObject, IEquatable<PdfName>, IComparable<PdfName>
{
    // Simple cache for frequently used standard names
    private static readonly Dictionary<string, PdfName> NameCache = [];

    public string Value { get; }

    // Private constructor, use factory method Get
    private PdfName(string value)
    {
        // Basic validation (can be expanded based on spec stricter rules if needed)
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("PDF name value cannot be null or empty.", nameof(value));
        // PDF spec 7.3.5: "shall not contain any of the delimiter characters ( ), <, >, [, ], {, }, /, or %"
        // However, escaping allows them. We store the raw value and escape on Write.
        // We should perhaps validate against NULL (character code 0) here.
        if (value.Contains('\0'))
            throw new ArgumentException("PDF name value cannot contain the NUL character.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Gets a PdfName instance for the specified string value, potentially using a cache.
    /// </summary>
    public static PdfName Get(string value)
    {
        if (!NameCache.TryGetValue(value, out var name))
        {
            name = new PdfName(value);
            // Only cache potentially common/short names to avoid memory bloat
            if (value.Length < 32) // Arbitrary limit
            {
                lock (NameCache) // Simple lock for thread safety if accessed concurrently
                {
                    // Double check locking pattern
                    NameCache.TryAdd(value, name);
                }
            }
        }
        return name;
    }

    internal override void Write(StreamWriter writer, Encoding encoding)
    {
        writer.Write('/');
        WriteEscaped(writer, Value, encoding);
    }

    /// <summary>
    /// Writes the name string with PDF escaping rules applied.
    /// </summary>
    internal static void WriteEscaped(StreamWriter writer, string value, Encoding encoding)
    {
        // As per PDF Spec 1.7, Section 7.3.5:
        // "any character except null (character code 0) may be included in a name
        // by writing its 2-digit hexadecimal code, preceded by the number sign character (#)."
        // Regular characters are ASCII 33 '!' to 126 '~', excluding delimiters and '#'.
        foreach (char c in value)
        {
            byte charCode = (byte)c; // Assumes simple mapping for common chars

            // Characters that MUST be escaped: delimiters, #, and non-regular ASCII
            // Delimiters: ( ) < > [ ] { } / %
            if (charCode < 33 || charCode > 126 ||
                c == '#' || c == '(' || c == ')' || c == '<' || c == '>' ||
                c == '[' || c == ']' || c == '{' || c == '}' || c == '/' || c == '%')
            {
                writer.Write($"#{charCode:X2}");
            }
            else
            {
                writer.Write(c);
            }
        }
    }

    // --- Equality and Comparison (based on the string value) ---
    public bool Equals(PdfName? other) => other != null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PdfName other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public int CompareTo(PdfName? other) => string.CompareOrdinal(Value, other?.Value); // Use Ordinal for byte-level compare

    public override string ToString() => "/" + Value; // For debugging

    // --- Standard PDF Names (add more as needed) ---
    public static readonly PdfName Type = Get("Type");
    public static readonly PdfName Pages = Get("Pages");
    public static readonly PdfName Page = Get("Page");
    public static readonly PdfName Catalog = Get("Catalog");
    public static readonly PdfName Kids = Get("Kids");
    public static readonly PdfName Parent = Get("Parent");
    public static readonly PdfName Count = Get("Count");
    public static readonly PdfName MediaBox = Get("MediaBox");
    public static readonly PdfName CropBox = Get("CropBox");
    public static readonly PdfName Rotate = Get("Rotate");
    public static readonly PdfName Resources = Get("Resources");
    public static readonly PdfName Contents = Get("Contents");
    public static readonly PdfName Font = Get("Font");
    public static readonly PdfName XObject = Get("XObject");
    public static readonly PdfName Length = Get("Length");
    public static readonly PdfName Filter = Get("Filter");
    public static readonly PdfName FlateDecode = Get("FlateDecode");
    public static readonly PdfName DCTDecode = Get("DCTDecode");
    public static readonly PdfName Subtype = Get("Subtype");
    public static readonly PdfName Image = Get("Image");
    public static readonly PdfName Width = Get("Width");
    public static readonly PdfName Height = Get("Height");
    public static readonly PdfName ColorSpace = Get("ColorSpace");
    public static readonly PdfName BitsPerComponent = Get("BitsPerComponent");
    public static readonly PdfName DeviceRGB = Get("DeviceRGB");
    public static readonly PdfName DeviceGray = Get("DeviceGray");
    public static readonly PdfName DeviceCMYK = Get("DeviceCMYK");
    public static readonly PdfName Indexed = Get("Indexed");
    public static readonly PdfName DecodeParms = Get("DecodeParms");
    public static readonly PdfName Predictor = Get("Predictor");
    public static readonly PdfName Columns = Get("Columns");

    public static readonly PdfName Size = Get("Size"); // Para Trailer
    public static readonly PdfName Root = Get("Root"); // Para Trailer
    public static readonly PdfName Info = Get("Info"); // Para Trailer (opcional)
    public static readonly PdfName ID = Get("ID");   // Para Trailer (opcional)
    public static readonly PdfName Encrypt = Get("Encrypt"); // Para Trailer (opcional)

    // Add other common names (ProcSet, ExtGState, Pattern, Shading, etc.)
}
