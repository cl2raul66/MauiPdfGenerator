using System.Collections;
using System.Text;

namespace MauiPdfGenerator.Core.Objects;

/// <summary>
/// Represents a PDF dictionary object - an unordered collection of key-value pairs
/// where keys are PdfNames and values are PdfObjects. Section 7.3.7.
/// </summary>
internal class PdfDictionary : PdfObject, IEnumerable<KeyValuePair<PdfName, PdfObject>>
{
    // Use a standard dictionary for storage. PDF Names ensure uniqueness.
    protected readonly Dictionary<PdfName, PdfObject> _entries = [];

    /// <summary>
    /// Gets the number of entries in the dictionary.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Initializes a new empty instance of the <see cref="PdfDictionary"/> class.
    /// </summary>
    public PdfDictionary() { }

    /// <summary>
    /// Gets or sets the PdfObject associated with the specified PdfName key.
    /// Getting a non-existent key returns null. Setting null removes the key.
    /// </summary>
    /// <param name="key">The PdfName key.</param>
    /// <returns>The associated PdfObject, or null if the key is not found.</returns>
    public PdfObject? this[PdfName key]
    {
        get => _entries.TryGetValue(key, out var value) ? value : null;
        set
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (value is null) // Treat setting null as removal
            {
                _entries.Remove(key);
            }
            else
            {
                _entries[key] = value;
            }
        }
    }

    /// <summary>
    /// Adds or updates an entry in the dictionary.
    /// </summary>
    /// <param name="key">The PdfName key.</param>
    /// <param name="value">The PdfObject value. Cannot be null.</param>
    public void Add(PdfName key, PdfObject value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (value is null) throw new ArgumentNullException(nameof(value), "Dictionary values cannot be null C# references. Use PdfNull.Instance.");
        _entries[key] = value;
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value, if found.</param>
    /// <returns>True if the key was found, otherwise false.</returns>
    public bool TryGetValue(PdfName key, out PdfObject? value)
    {
        return _entries.TryGetValue(key, out value);
    }

    /// <summary>
    /// Checks if the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key exists, otherwise false.</returns>
    public bool ContainsKey(PdfName key)
    {
        return _entries.ContainsKey(key);
    }

    /// <summary>
    /// Removes the entry with the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>True if the key was found and removed, otherwise false.</returns>
    public bool Remove(PdfName key)
    {
        return _entries.Remove(key);
    }


    internal override void Write(StreamWriter writer, Encoding encoding)
    {
        writer.WriteLine("<<"); // Start dictionary

        foreach (var kvp in _entries)
        {
            kvp.Key.Write(writer, encoding); // Write key (/Name)
            writer.Write(' ');
            kvp.Value.Write(writer, encoding); // Write value
            writer.WriteLine(); // Newline for readability (optional but common)
        }

        writer.Write(">>"); // End dictionary
    }

    internal override async Task WriteAsync(Stream stream, Encoding encoding)
    {
        await stream.WriteAsync(Encoding.ASCII.GetBytes("<<\n"), 0, 3); // Start dictionary + newline

        foreach (var kvp in _entries)
        {
            await kvp.Key.WriteAsync(stream, encoding); // Write key (/Name)
            await stream.WriteAsync(Encoding.ASCII.GetBytes(" "), 0, 1);
            await kvp.Value.WriteAsync(stream, encoding); // Write value
            await stream.WriteAsync(Encoding.ASCII.GetBytes("\n"), 0, 1); // Newline
        }

        await stream.WriteAsync(Encoding.ASCII.GetBytes(">>"), 0, 2); // End dictionary
    }


    // --- IEnumerable Implementation ---
    public IEnumerator<KeyValuePair<PdfName, PdfObject>> GetEnumerator() => _entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"<<{_entries.Count} entries>>"; // Simple debug string
}
