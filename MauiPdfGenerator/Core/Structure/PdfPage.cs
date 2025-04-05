using MauiPdfGenerator.Common.Geometry;
using MauiPdfGenerator.Core.Objects;

namespace MauiPdfGenerator.Core.Structure;

/// <summary>
/// Represents a page object dictionary (/Page). Section 7.7.3.
/// Defines attributes and content for a single page.
/// </summary>
internal class PdfPage : PdfDictionary
{
    private readonly PdfDocument _document;

    /// <summary>
    /// Gets or sets the reference to the parent node in the page tree. REQUIRED.
    /// </summary>
    public PdfReference Parent
    {
        get => this[PdfName.Parent] as PdfReference ?? throw new InvalidOperationException("Page /Parent reference is missing or invalid.");
        internal set // Only settable internally during tree construction
        {
            if (value is null) throw new ArgumentNullException(nameof(value), "/Parent entry cannot be null for a Page.");
            Add(PdfName.Parent, value);
        }
    }

    /// <summary>
    /// Gets or sets the rectangle defining the boundaries of the physical medium (/MediaBox). REQUIRED.
    /// Expressed in default user space units. Inheritable.
    /// </summary>
    public PdfRectangle MediaBox
    {
        // TODO: Implement inheritance lookup if not set directly
        get => GetRectangle(PdfName.MediaBox) ?? throw new InvalidOperationException("Page /MediaBox is missing.");
        set => Add(PdfName.MediaBox, CreatePdfArray(value));
    }

    /// <summary>
    /// Gets or sets the rectangle defining the visible region (/CropBox). Optional.
    /// Defaults to MediaBox if not specified. Inheritable.
    /// </summary>
    public PdfRectangle? CropBox
    {
        // TODO: Implement inheritance lookup
        get => GetRectangle(PdfName.CropBox);
        set => AddOrRemoveRectangle(PdfName.CropBox, value);
    }

    /// <summary>
    /// Gets or sets the page rotation in degrees (multiple of 90). Optional.
    /// Values: 0, 90, 180, 270. Inheritable.
    /// </summary>
    public int Rotate
    {
        // TODO: Implement inheritance lookup
        get => (int?)(this[PdfName.Rotate] as PdfNumber)?.Value ?? 0;
        set
        {
            if (value % 90 != 0) throw new ArgumentException("Rotate value must be a multiple of 90.", nameof(value));
            // Normalize value? (e.g., 360 becomes 0) - PDF spec allows any multiple
            if (value == 0) Remove(PdfName.Rotate); // Remove if default
            else Add(PdfName.Rotate, new PdfNumber(value));
        }
    }

    /// <summary>
    /// Gets or sets the reference to the resource dictionary for this page. REQUIRED if page has content.
    /// Inheritable.
    /// </summary>
    public PdfDictionary? Resources // Changed to PdfDictionary from PdfReference for direct access during page build
    {
        // TODO: Implement inheritance lookup
        get => this[PdfName.Resources] as PdfDictionary; // May need dereferencing if stored as reference
        set => AddOrRemove(PdfName.Resources, value);
    }


    /// <summary>
    /// Gets or sets the reference to the content stream or an array of content streams. REQUIRED.
    /// </summary>
    public PdfObject Contents // Can be PdfReference (to a stream) or PdfArray (of references)
    {
        get => this[PdfName.Contents] ?? throw new InvalidOperationException("Page /Contents is missing.");
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value), "/Contents entry cannot be null.");
            // Validate type? Should be PdfReference or PdfArray of PdfReference
            Add(PdfName.Contents, value);
        }
    }

    // Add other optional entries like /Annots, /Thumb, /BleedBox, /TrimBox, /ArtBox, /StructParents etc. as needed

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfPage"/> class.
    /// </summary>
    /// <param name="document">The parent document.</param>
    /// <param name="mediaBox">The page media box.</param>
    /// <param name="parentRef">Reference to the parent page tree node.</param>
    internal PdfPage(PdfDocument document, PdfRectangle mediaBox, PdfReference parentRef) : base()
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));

        Add(PdfName.Type, PdfName.Page); // Required /Type entry
        Parent = parentRef;             // Required /Parent entry
        MediaBox = mediaBox;            // Required /MediaBox entry

        // Initialize required Resources dictionary (will be populated later)
        Resources = new PdfDictionary(); // Start with an empty dictionary

        // Contents will be set later when the content stream is created
    }


    // --- Helper Methods ---
    private static PdfArray CreatePdfArray(PdfRectangle rect)
    {
        // PDF rectangle: [lower-left-x lower-left-y upper-right-x upper-right-y]
        // Our PdfRectangle origin is top-left, PDF coordinate system origin is bottom-left usually.
        // For now, assume rect coordinates are already in PDF space (LLX, LLY, URX, URY?)
        // Let's assume our PdfRectangle uses PDF coordinates for MediaBox/CropBox directly.
        // LLX = rect.Left, LLY = rect.Bottom (if Y increases upwards), URX = rect.Right, URY = rect.Top
        // Common PDF convention: Y increases upwards from bottom-left origin.
        // Our PdfRectangle: X, Y (TopLeft), Width, Height
        // We need: [X, Y+Height, X+Width, Y] ? NO, PDF origin is Bottom-Left!
        // PDF Spec: [LLx LLy URx URy]
        // So: [rect.Left, rect.Bottom, rect.Right, rect.Top] assuming our Y increases upwards for rect.
        // BUT our rect assumes Y increases DOWNWARDS (Top, Bottom properties).
        // --> Let's assume the passed PdfRectangle ALREADY uses the PDF coordinate system (origin bottom-left, Y increases up)
        // --> So, LLx = rect.X, LLy = rect.Y, URx = rect.X + rect.Width, URy = rect.Y + rect.Height
        return new PdfArray(
            new PdfNumber(rect.X),                 // LLx
            new PdfNumber(rect.Y),                 // LLy
            new PdfNumber(rect.X + rect.Width),    // URx
            new PdfNumber(rect.Y + rect.Height)    // URy
        );
    }

    private PdfRectangle? GetRectangle(PdfName key)
    {
        if (this[key] is PdfArray arr && arr.Count == 4 &&
            arr.ElementAtOrDefault(0) is PdfNumber n1 &&
            arr.ElementAtOrDefault(1) is PdfNumber n2 &&
            arr.ElementAtOrDefault(2) is PdfNumber n3 &&
            arr.ElementAtOrDefault(3) is PdfNumber n4)
        {
            // Assuming array is [LLx LLy URx URy]
            double llx = n1.Value;
            double lly = n2.Value;
            double urx = n3.Value;
            double ury = n4.Value;
            // Convert back to our PdfRectangle (X, Y, Width, Height) assuming bottom-left origin
            return new PdfRectangle(llx, lly, urx - llx, ury - lly);
        }
        return null;
    }

    private void AddOrRemoveRectangle(PdfName key, PdfRectangle? value)
    {
        if (value.HasValue) Add(key, CreatePdfArray(value.Value));
        else Remove(key);
    }

    private void AddOrRemove(PdfName key, PdfObject? value)
    {
        if (value == null || value is PdfNull) Remove(key);
        else Add(key, value);
    }

}
