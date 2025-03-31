using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Core.ObjectModel;

internal class PdfName : PdfObject
{
    private readonly string _name; // Must start with '/'

    public PdfName(string name)
    {
        if (string.IsNullOrEmpty(name) || name[0] != '/')
            throw new ArgumentException("PDF Name must start with '/' and not be empty.", nameof(name));

        // TODO: Implement proper escaping of special characters (#xx) in names if needed
        _name = name;
    }

    public string Value => _name;

    public override async Task WriteAsync(Stream stream, PdfWriter? writer = null)
    {
        // Assume name is already correctly formatted/escaped for now
        await WriteBytesAsync(stream, PdfEncodings.StructureEncoding.GetBytes(_name));
    }
}
