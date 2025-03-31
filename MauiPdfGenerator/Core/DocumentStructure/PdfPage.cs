using MauiPdfGenerator.Core.ObjectModel;

namespace MauiPdfGenerator.Core.DocumentStructure;

/// <summary>
/// Represents a PDF Page object (/Page dictionary).
/// Internal implementation detail.
/// </summary>
internal class PdfPage : PdfDictionary
{
    /// <summary>
    /// Creates a new Page dictionary.
    /// </summary>
    /// <param name="parentReference">Reference to the parent Pages node.</param>
    /// <param name="mediaBox">The page dimensions [0 0 Width Height].</param>
    /// <param name="resourcesReference">Reference to the page's resources dictionary.</param>
    /// <param name="contentsReference">Reference to the page's content stream(s).</param>
    public PdfPage(PdfReference parentReference, PdfArray mediaBox, PdfReference resourcesReference, PdfReference contentsReference)
    {
        this[Common.PdfConstants.Names.Type] = new PdfName(Common.PdfConstants.Names.Page);
        this[Common.PdfConstants.Names.Parent] = parentReference ?? throw new ArgumentNullException(nameof(parentReference));
        this[Common.PdfConstants.Names.MediaBox] = mediaBox ?? throw new ArgumentNullException(nameof(mediaBox));
        this[Common.PdfConstants.Names.Resources] = resourcesReference ?? throw new ArgumentNullException(nameof(resourcesReference));
        this[Common.PdfConstants.Names.Contents] = contentsReference ?? throw new ArgumentNullException(nameof(contentsReference));
        // Optional: /CropBox, /BleedBox, /TrimBox, /ArtBox, /Annots, etc.
    }

    /// <summary>
    /// Creates a new Page dictionary with content stream array support.
    /// </summary>
    public PdfPage(PdfReference parentReference, PdfArray mediaBox, PdfReference resourcesReference, PdfArray contentsArray)
    {
        this[Common.PdfConstants.Names.Type] = new PdfName(Common.PdfConstants.Names.Page);
        this[Common.PdfConstants.Names.Parent] = parentReference ?? throw new ArgumentNullException(nameof(parentReference));
        this[Common.PdfConstants.Names.MediaBox] = mediaBox ?? throw new ArgumentNullException(nameof(mediaBox));
        this[Common.PdfConstants.Names.Resources] = resourcesReference ?? throw new ArgumentNullException(nameof(resourcesReference));
        this[Common.PdfConstants.Names.Contents] = contentsArray ?? throw new ArgumentNullException(nameof(contentsArray));
    }
}
