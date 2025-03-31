using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Core.ObjectModel;

internal class PdfNull : PdfObject
{
    public static readonly PdfNull Instance = new();
    private PdfNull() { } // Singleton

    public override async Task WriteAsync(Stream stream, PdfWriter? writer = null)
    {
        await WriteBytesAsync(stream, Common.PdfConstants.NullKeyword);
    }
}
