using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using System.Collections;
using MauiPdfGenerator.Fluent.Enums; 

namespace MauiPdfGenerator.Core.Integration;

internal class PdfAwareFontCollection : IFontCollection
{
    private readonly IFontCollection _effectiveMauiFontCollection;
    private readonly PdfFontRegistryBuilder _pdfFontRegistryBuilder;
    private readonly FontDestinationType _destination;
    public PdfAwareFontCollection(
        IFontCollection originalMauiFontCollection,
        PdfFontRegistryBuilder pdfFontRegistryBuilder,
        FontDestinationType destination)
    {
        _pdfFontRegistryBuilder = pdfFontRegistryBuilder;
        _destination = destination;

        if (_destination == FontDestinationType.OnlyPDF)
        {
            _effectiveMauiFontCollection = new DummyFontCollection();
        }
        else
        {
            _effectiveMauiFontCollection = originalMauiFontCollection;
        }
    }

    public void AddFont(string filename, string? alias = null)
    {
        string effectiveAlias = alias ?? Path.GetFileNameWithoutExtension(filename);
        if (string.IsNullOrEmpty(effectiveAlias))
        {
            System.Diagnostics.Debug.WriteLine($"[MauiPdfGenerator] Advertencia: Imposible registrar la fuente '{filename}' sin un alias válido.");
            return;
        }

        if (_destination == FontDestinationType.Both || _destination == FontDestinationType.OnlyUI)
        {
            _effectiveMauiFontCollection.AddFont(filename, alias);
        }

        if (_destination == FontDestinationType.Both || _destination == FontDestinationType.OnlyPDF)
        {
            var identifier = new PdfFontIdentifier(effectiveAlias);
            _pdfFontRegistryBuilder.GetOrCreateFontRegistration(identifier, filename, isFromMauiConfig: true);
        }
    }

    public FontDescriptor this[int index]
    {
        get => _effectiveMauiFontCollection[index];
        set => _effectiveMauiFontCollection[index] = value;
    }

    public int Count => _effectiveMauiFontCollection.Count;

    public bool IsReadOnly => _effectiveMauiFontCollection.IsReadOnly;

    public void Add(FontDescriptor item)
    {
        if (_destination == FontDestinationType.Both || _destination == FontDestinationType.OnlyUI)
        {
            _effectiveMauiFontCollection.Add(item);
        }
        if ((_destination == FontDestinationType.Both || _destination == FontDestinationType.OnlyPDF) && !string.IsNullOrEmpty(item.Alias))
        {
            var identifier = new PdfFontIdentifier(item.Alias);
            _pdfFontRegistryBuilder.GetOrCreateFontRegistration(identifier, item.Filename, isFromMauiConfig: true);
        }
    }

    public void Clear()
    {
        if (_destination == FontDestinationType.Both || _destination == FontDestinationType.OnlyUI)
        {
            _effectiveMauiFontCollection.Clear();
        }
    }

    public bool Contains(FontDescriptor item) => _effectiveMauiFontCollection.Contains(item);

    public void CopyTo(FontDescriptor[] array, int arrayIndex) => _effectiveMauiFontCollection.CopyTo(array, arrayIndex);

    public IEnumerator<FontDescriptor> GetEnumerator() => _effectiveMauiFontCollection.GetEnumerator();

    public int IndexOf(FontDescriptor item) => _effectiveMauiFontCollection.IndexOf(item);

    public void Insert(int index, FontDescriptor item)
    {
        if (_destination == FontDestinationType.Both || _destination == FontDestinationType.OnlyUI)
        {
            _effectiveMauiFontCollection.Insert(index, item);
        }
        if ((_destination == FontDestinationType.Both || _destination == FontDestinationType.OnlyPDF) && !string.IsNullOrEmpty(item.Alias))
        {
            var identifier = new PdfFontIdentifier(item.Alias);
            _pdfFontRegistryBuilder.GetOrCreateFontRegistration(identifier, item.Filename, isFromMauiConfig: true);
        }
    }

    public bool Remove(FontDescriptor item)
    {
        bool removed = false;
        if (_destination == FontDestinationType.Both || _destination == FontDestinationType.OnlyUI)
        {
            removed = _effectiveMauiFontCollection.Remove(item);
        }
        return removed;
    }

    public void RemoveAt(int index)
    {
        if (_destination == FontDestinationType.Both || _destination == FontDestinationType.OnlyUI)
        {
            _effectiveMauiFontCollection.RemoveAt(index);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => _effectiveMauiFontCollection.GetEnumerator();

    private class DummyFontCollection : IFontCollection
    {
        private readonly List<FontDescriptor> _list = new List<FontDescriptor>();
        public FontDescriptor this[int index] { get => _list[index]; set => _list[index] = value; }
        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public void Add(FontDescriptor item) => _list.Add(item);

        public void AddFont(string filename, string? alias = null)
        {
            _list.Add(new FontDescriptor(filename, alias, null));
        }

        public void Clear() => _list.Clear();
        public bool Contains(FontDescriptor item) => _list.Contains(item);
        public void CopyTo(FontDescriptor[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public IEnumerator<FontDescriptor> GetEnumerator() => _list.GetEnumerator();
        public int IndexOf(FontDescriptor item) => _list.IndexOf(item);
        public void Insert(int index, FontDescriptor item) => _list.Insert(index, item);
        public bool Remove(FontDescriptor item) => _list.Remove(item);
        public void RemoveAt(int index) => _list.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
