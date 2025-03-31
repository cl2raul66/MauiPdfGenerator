using MauiPdfGenerator.Common;
using MauiPdfGenerator.Core.ObjectModel;

namespace MauiPdfGenerator.Core.DocumentStructure;

/// <summary>
/// Represents the PDF Document Catalog (/Catalog dictionary).
/// Internal implementation detail.
/// </summary>
internal class PdfCatalog : PdfDictionary
{
    public PdfCatalog(PdfReference pagesRootNode)
    {
        this[PdfConstants.Names.Type] = new PdfName(PdfConstants.Names.Catalog);
        this[PdfConstants.Names.Pages] = pagesRootNode ?? throw new ArgumentNullException(nameof(pagesRootNode));
        // Optionally add /Version, /Metadata, etc. here later
    }
}
