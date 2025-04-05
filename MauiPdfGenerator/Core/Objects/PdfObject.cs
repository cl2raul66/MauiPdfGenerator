using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Abstract base class for all PDF object types (Section 7.3).
/// Marked internal as it's an implementation detail of the Core engine.
/// </summary>
internal abstract class PdfObject
{
    /// <summary>
    /// Writes the PDF representation of this object to the specified writer.
    /// Implementations must conform to the PDF specification syntax.
    /// </summary>
    /// <param name="writer">The writer to write the PDF syntax to.</param>
    /// <param name="encoding">The encoding to use, typically ASCII or a compatible one for PDF structure.</param>
    internal abstract void Write(StreamWriter writer, Encoding encoding);

    /// <summary>
    /// Asynchronously writes the PDF representation of this object to the specified stream.
    /// Implementations must conform to the PDF specification syntax.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="encoding">The encoding to use, typically ASCII or a compatible one for PDF structure.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    internal virtual async Task WriteAsync(Stream stream, Encoding encoding)
    {
        // Default implementation using StreamWriter for non-stream types.
        // PdfStream will override this for efficiency.
        // Use leaveOpen: true to prevent StreamWriter from closing the underlying stream.
        using (var writer = new StreamWriter(stream, encoding, bufferSize: 1024, leaveOpen: true))
        {
            Write(writer, encoding);
            await writer.FlushAsync(); // Ensure buffered content is written to the stream
        }
    }

    // Consider adding a GetBytes(Encoding encoding) method later if needed often.
}
