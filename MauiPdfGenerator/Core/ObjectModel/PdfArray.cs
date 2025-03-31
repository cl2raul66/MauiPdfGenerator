using MauiPdfGenerator.Common;
using System.Collections;

namespace MauiPdfGenerator.Core.ObjectModel;

internal class PdfArray : PdfObject, IEnumerable<PdfObject>
{
    private readonly List<PdfObject> _items = [];

    public PdfArray() { }
    public PdfArray(IEnumerable<PdfObject> items) => _items.AddRange(items);

    public void Add(PdfObject item) => _items.Add(item);
    public void Clear() => _items.Clear();

    public IEnumerator<PdfObject> GetEnumerator() => _items.GetEnumerator();

    public override async Task WriteAsync(Stream stream, PdfWriter? writer = null)
    {
        if (writer is null)
        {
            return;
        }
        await WriteBytesAsync(stream, Common.PdfConstants.ArrayStart);
        bool first = true;
        foreach (var item in _items)
        {
            if (!first)
                await WriteBytesAsync(stream, Common.PdfConstants.Space);
            await item.WriteAsync(stream, writer);
            first = false;
        }
        await WriteBytesAsync(stream, Common.PdfConstants.ArrayEnd);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
