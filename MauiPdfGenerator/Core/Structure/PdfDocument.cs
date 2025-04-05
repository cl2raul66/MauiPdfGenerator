using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Common.Geometry;

namespace MauiPdfGenerator.Core.Structure;

/// <summary>
/// Represents the overall PDF document being constructed.
/// Manages object allocation, the document structure (Catalog, Pages),
/// and orchestrates the writing process.
/// </summary>
internal class PdfDocument
{
    private readonly List<PdfIndirectObject> _objects = new List<PdfIndirectObject>();
    private int _nextObjectId = 1; // Object IDs start at 1

    // --- Core Structure References ---
    public PdfCatalog Catalog { get; }
    public PdfInfo Info { get; }
    public PdfPageTreeNode PageTreeRoot { get; }
    private readonly PdfReference _pageTreeRootRef; // Stores the reference to the PageTreeRoot indirect object

    // Default page size, margins etc can be stored here, derived from Fluent API config
    public PdfRectangle DefaultMediaBox { get; set; } // Example

    // TODO: Add Security Handler reference if encryption is used

    public PdfDocument()
    {
        // TODO: Initialize DefaultMediaBox based on config (e.g., A4 from Common or Fluent config)
        // Example: DefaultMediaBox = PdfPageSizes.A4; // Assuming a static class PdfPageSizes exists

        // Create essential structures
        Info = new PdfInfo();
        PageTreeRoot = new PdfPageTreeNode(this);

        // Create the indirect object for the PageTreeRoot *first* and store its reference
        var pageTreeRootIndirect = AddIndirectObject(PageTreeRoot);
        _pageTreeRootRef = pageTreeRootIndirect.Reference;

        // Now create the Catalog using the obtained reference
        Catalog = new PdfCatalog(this, _pageTreeRootRef);

        // **** NUEVO: Añadir Catalog como objeto indirecto ****
        AddIndirectObject(Catalog); // Aseguramos que Catalog tenga un ID

        // Info dictionary is added later during writing process if needed
    }

    /// <summary>
    /// Adds a direct PdfObject to the document as an indirect object,
    /// assigning it a unique ID and generation number.
    /// </summary>
    /// <param name="directObject">The object to add.</param>
    /// <returns>The created PdfIndirectObject wrapper.</returns>
    public PdfIndirectObject AddIndirectObject(PdfObject directObject)
    {
        if (directObject == null) throw new ArgumentNullException(nameof(directObject));

        // Prevent adding the same direct object instance multiple times inadvertently
        // Although sometimes intentional re-use might occur (e.g. shared resource dicts).
        // This check might need refinement based on use cases.
        var existingIndirect = _objects.FirstOrDefault(io => ReferenceEquals(io.Value, directObject));
        if (existingIndirect != null)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Attempted to add the same direct object instance twice (Type: {directObject.GetType().Name}). Re-using existing indirect object {existingIndirect.Reference}.");
            return existingIndirect;
        }


        var indirectObject = new PdfIndirectObject(directObject)
        {
            Id = _nextObjectId++,
            Generation = 0 // Generation is always 0 for new objects in a new document
        };
        _objects.Add(indirectObject);
        return indirectObject;
    }

    /// <summary>
    /// Gets all indirect objects managed by this document.
    /// </summary>
    public IEnumerable<PdfIndirectObject> GetIndirectObjects() => _objects;


    /// <summary>
    /// Adds a new page to the document's page tree.
    /// Ensures the page is added as an indirect object and linked correctly.
    /// </summary>
    /// <param name="page">The PdfPage object to add (must not be null).</param>
    public void AddPage(PdfPage page)
    {
        if (page == null) throw new ArgumentNullException(nameof(page));

        // Ensure the page dictionary itself is added as an indirect object
        var pageIndirect = AddIndirectObject(page);

        // Add the page's reference to the PageTreeRoot's /Kids array
        // The PdfPageTreeNode.AddChild handles updating its internal list and /Count
        PageTreeRoot.AddChild(pageIndirect.Reference, 1); // Assuming adding one leaf page

        // Set the /Parent reference *inside the page's dictionary* to point to the PageTreeRoot's indirect object
        page.Parent = _pageTreeRootRef; // <-- Use the stored reference here

        // Update the parent reference within the PageTreeRoot dictionary itself
        // (This ensures /Parent points correctly if PageTreeRoot had a parent, or is removed if it's the root)
        PageTreeRoot.SetParentReference();
    }


    // --- Methods for Writing the Document ---
    // public Task WriteAsync(Stream stream, PdfWriterOptions options = null)
    // {
    //     // ... Implementation using PdfWriter ...
    //     throw new NotImplementedException();
    // }
}
