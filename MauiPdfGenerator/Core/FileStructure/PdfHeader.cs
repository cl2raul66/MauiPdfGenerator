using System.Text;

namespace MauiPdfGenerator.Core.FileStructure;

/// <summary>
/// Handles writing the PDF file header.
/// Internal implementation detail.
/// </summary>
internal record PdfHeader
{
    // PDF 1.7 is a reasonable modern choice. PDF 2.0 requires more features.
    private static readonly byte[] HeaderBytes = Encoding.ASCII.GetBytes("%PDF-1.7\n");
    // Add a comment with high-bit bytes to indicate binary content, as recommended.
    private static readonly byte[] BinaryCommentBytes = GetBinaryCommentBytes();

    /// <summary>
    /// Writes the standard PDF header and binary comment to the stream.
    /// </summary>
    public static async Task WriteAsync(Stream stream)
    {
        await stream.WriteAsync(HeaderBytes);
        await stream.WriteAsync(BinaryCommentBytes);
    }

    private static byte[] GetBinaryCommentBytes()
    {
        byte[] markerBytes = new byte[6];
        Random random = new();

        markerBytes[0] = (byte)'%';

        // Generar 4 bytes aleatorios con valores altos
        for (int i = 1; i < 5; i++)
        {
            byte randomByte = (byte)random.Next(128, 256);
            markerBytes[i] = randomByte;
        }

        markerBytes[5] = (byte)'\n';

        return markerBytes;
    }
}
