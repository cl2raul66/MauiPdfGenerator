using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Core.ObjectModel;

internal class PdfReal : PdfObject
{
    private readonly double _value;
    public PdfReal(double value) => _value = value;

    public override async Task WriteAsync(Stream stream, PdfWriter? writer = null)
    {
        // Use InvariantCulture formatting
        await WriteBytesAsync(stream, Common.PdfEncodings.StructureEncoding.GetBytes(
            _value.ToString("0.#####", System.Globalization.CultureInfo.InvariantCulture)));
    }
}
