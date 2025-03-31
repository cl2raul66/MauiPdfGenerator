using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Core.ObjectModel;

internal class PdfInteger : PdfObject
{
    private readonly long _value;
    public PdfInteger(long value) => _value = value;

    public override async Task WriteAsync(Stream stream, PdfWriter? writer = null)
    {
        await WriteBytesAsync(stream, Common.PdfEncodings.StructureEncoding.GetBytes(_value.ToString()));
    }
}
