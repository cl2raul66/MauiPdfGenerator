using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Core.ObjectModel;

/// <summary>
/// Base class for all PDF objects (integer, string, dictionary, stream, etc.).
/// Internal as the object model is an implementation detail.
/// </summary>
internal abstract class PdfObject
{
    /// <summary>
    /// Asynchronously writes the byte representation of this PDF object to the stream.
    /// This includes delimiters like '&lt;&lt; >>' or '[ ]' but *not* 'obj'/'endobj' keywords.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="writer">Optional PdfWriter context for indirect object lookup.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    public abstract Task WriteAsync(Stream stream, PdfWriter? writer = null);

    // Helper method for writing simple byte arrays
    protected async Task WriteBytesAsync(Stream stream, byte[] data)
    {
        if (data is not null && data.Length > 0)
        {
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}
