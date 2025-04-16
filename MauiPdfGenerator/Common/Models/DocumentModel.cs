namespace MauiPdfGenerator.Common;

/// <summary>
/// Represents the entire PDF document structure, including settings and pages,
/// before generation by the Core engine. This model is built by the Fluent API.
/// </summary>
internal class DocumentModel
{
    /// <summary>
    /// Global settings and metadata for the document.
    /// </summary>
    public DocumentSettings Settings { get; set; }

    /// <summary>
    /// The ordered list of pages in the document.
    /// </summary>
    public List<PageModel> Pages { get; set; }

    public DocumentModel()
    {
        // Initialize with default settings and an empty page list
        Settings = new DocumentSettings();
        Pages = [];
    }
}
