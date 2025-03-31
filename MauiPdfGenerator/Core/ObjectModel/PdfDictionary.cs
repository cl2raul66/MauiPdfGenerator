using MauiPdfGenerator.Common;
using System.Collections;

namespace MauiPdfGenerator.Core.ObjectModel;

/// <summary>
/// Represents a PDF Dictionary object '&lt;&lt; ... >>'.
/// Internal as it's an implementation detail.
/// </summary>
internal class PdfDictionary : PdfObject, IEnumerable<KeyValuePair<PdfName, PdfObject>>
{
    // Use SortedDictionary to write keys in a consistent (alphabetical) order, useful for debugging/comparison
    private readonly SortedDictionary<string, PdfObject> _items = [];

    public PdfDictionary() { }

    public PdfObject this[PdfName key]
    {
        get => _items.TryGetValue(key.Value, out var value) ? value : PdfNull.Instance;
        set
        {
            if (value is null || value is PdfNull)
                _items.Remove(key.Value);
            else
                _items[key.Value] = value;
        }
    }

    public PdfObject this[string keyName]
    {
        get => this[new PdfName(keyName)];
        set => this[new PdfName(keyName)] = value;
    }

    public bool ContainsKey(PdfName key) => _items.ContainsKey(key.Value);
    public bool ContainsKey(string keyName) => _items.ContainsKey(keyName);

    public override async Task WriteAsync(Stream stream, PdfWriter? writer = null)
    {
        await WriteBytesAsync(stream, PdfConstants.DictStart);
        await WriteBytesAsync(stream, Common.PdfConstants.NewLine); // For readability

        foreach (var kvp in _items)
        {
            // Write Key (PdfName)
            await (new PdfName(kvp.Key)).WriteAsync(stream, writer);
            await WriteBytesAsync(stream, Common.PdfConstants.Space);

            // Write Value (PdfObject)
            await kvp.Value.WriteAsync(stream, writer);
            await WriteBytesAsync(stream, Common.PdfConstants.NewLine); // For readability
        }

        await WriteBytesAsync(stream, Common.PdfConstants.DictEnd);
    }

    public IEnumerator<KeyValuePair<PdfName, PdfObject>> GetEnumerator()
    {
        // Wrap the string key from SortedDictionary back into PdfName for the enumerator
        foreach (var kvp in _items)
        {
            yield return new KeyValuePair<PdfName, PdfObject>(new PdfName(kvp.Key), kvp.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
