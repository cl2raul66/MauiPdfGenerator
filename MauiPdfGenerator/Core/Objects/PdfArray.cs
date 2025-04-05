using System.Collections;
using System.Text;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Represents a PDF array object - an ordered collection of PdfObjects. Section 7.3.6.
/// </summary>
internal sealed class PdfArray : PdfObject, IEnumerable<PdfObject>
{
    private readonly List<PdfObject> _items = [];

    /// <summary>
    /// Gets the number of items in the array.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Initializes a new empty instance of the <see cref="PdfArray"/> class.
    /// </summary>
    public PdfArray() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfArray"/> class with initial items.
    /// </summary>
    /// <param name="items">The items to add to the array.</param>
    public PdfArray(IEnumerable<PdfObject> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items.AddRange(items);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfArray"/> class with initial items.
    /// </summary>
    /// <param name="items">The items to add to the array.</param>
    public PdfArray(params PdfObject[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _items.AddRange(items);
    }

    /// <summary>
    /// Adds a PdfObject to the end of the array.
    /// </summary>
    /// <param name="item">The object to add. Cannot be null.</param>
    public void Add(PdfObject item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item), "PDF arrays cannot contain null C# references. Use PdfNull.Instance instead.");
        _items.Add(item);
    }

    internal override void Write(StreamWriter writer, Encoding encoding)
    {
        writer.Write('[');
        bool first = true;
        foreach (var item in _items)
        {
            if (!first)
            {
                writer.Write(' '); // Space separator
            }
            item.Write(writer, encoding);
            first = false;
        }
        writer.Write(']');
    }

    internal override async Task WriteAsync(Stream stream, Encoding encoding)
    {
        // Slightly more optimized write for arrays (reduces StreamWriter allocations)
        await stream.WriteAsync(Encoding.ASCII.GetBytes("["), 0, 1); // ASCII is fine for delimiters
        bool first = true;
        foreach (var item in _items)
        {
            if (!first)
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes(" "), 0, 1);
            }
            await item.WriteAsync(stream, encoding); // Delegate async write to item
            first = false;
        }
        await stream.WriteAsync(Encoding.ASCII.GetBytes("]"), 0, 1);
    }


    // --- IEnumerable Implementation ---
    public IEnumerator<PdfObject> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"[{_items.Count} items]"; // Simple debug string
}
