using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Core.ObjectModel;

internal class PdfBoolean : PdfObject
{
    private readonly bool _value;
    public PdfBoolean(bool value) => _value = value;

    public override async Task WriteAsync(Stream stream, PdfWriter? writer = null)
    {
        await WriteBytesAsync(stream, _value ? PdfConstants.TrueKeyword : PdfConstants.FalseKeyword);
    }
}

