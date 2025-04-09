using MauiPdfGenerator.Common.Geometry;
using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Content;

namespace MauiPdfGenerator.Core.Structure;

/// <summary>
/// Represents a page object dictionary (/Page). Section 7.7.3.
/// Defines attributes and content for a single page.
/// </summary>
internal class PdfPage : PdfDictionary
{
    private readonly PdfDocument _document; 

    internal PdfResources PageResources => _pageResources;
    private readonly PdfResources _pageResources; // Mover la declaración aquí

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

    public PdfRectangle MediaBox
    {
        // Usa el getter que convierte DESDE PDF Array
        get => GetRectangle(PdfName.MediaBox) ?? throw new InvalidOperationException("Page /MediaBox is missing or invalid.");
        // Usa el setter que convierte HACIA PDF Array
        set => Add(PdfName.MediaBox, CreatePdfArray(value)); // value está en UI Coords
    }

    public PdfRectangle? CropBox
    {
        get => GetRectangle(PdfName.CropBox); // Usa el getter que convierte DESDE PDF Array
        set => AddOrRemoveRectangle(PdfName.CropBox, value); // value está en UI Coords
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

        // !! Importante: Establecer MediaBox PRIMERO usando el método que hace la conversión !!
        this.MediaBox = mediaBox; // Usa el setter, que llama a CreatePdfArray

        // Ahora es seguro usar this.MediaBox en otras conversiones si fuera necesario

        // Initialize required Resources dictionary
        _pageResources = new PdfResources(document); // PdfResources needs PdfDocument reference
        Add(PdfName.Resources, _pageResources); // Add the dictionary directly for now? Or add indirectly? Check PdfDocument.AddPage

        // Contents will be set later when the content stream is created
    }

    // --- Helper Methods ---
    private PdfArray CreatePdfArray(PdfRectangle rect) // Recibe rect en coords UI (Top-Left)
    {
        // Necesitamos la altura total del MediaBox para la conversión de Y
        double mediaBoxHeight = this.MediaBox.Height; // Asume que MediaBox ya está establecido

        // PDF rectangle: [LLx LLy URx URy]
        double llx = rect.X;
        // LLY: El Y de PDF es la distancia desde el borde inferior.
        // Es igual a AlturaTotal - Y_UI_Top - AlturaRect
        // O también: AlturaTotal - Y_UI_Bottom
        double lly = mediaBoxHeight - rect.Bottom;
        double urx = rect.Right;
        // URY: El Y de PDF es la distancia desde el borde inferior.
        // Es igual a AlturaTotal - Y_UI_Top
        double ury = mediaBoxHeight - rect.Top;

        // Asegurarse que lly <= ury
        if (lly > ury)
        {
            // Esto podría pasar si height es negativo, aunque el constructor de PdfRectangle lo impide.
            // O si mediaBoxHeight es incorrecto. Por seguridad, intercambiamos.
            (lly, ury) = (ury, lly);
            Console.WriteLine("Warning: Corrected inverted Y coordinates in CreatePdfArray for PdfPage.");
        }


        return new PdfArray(
            new PdfNumber(llx),
            new PdfNumber(lly),
            new PdfNumber(urx),
            new PdfNumber(ury)
        );
    }

    private PdfRectangle? GetRectangle(PdfName key)
    {
        if (this[key] is PdfArray arr && arr.Count == 4 &&
            arr.ElementAtOrDefault(0) is PdfNumber n1 && // LLx
            arr.ElementAtOrDefault(1) is PdfNumber n2 && // LLy
            arr.ElementAtOrDefault(2) is PdfNumber n3 && // URx
            arr.ElementAtOrDefault(3) is PdfNumber n4)   // URy
        {
            // Necesitamos la altura del MediaBox para la conversión inversa
            double mediaBoxHeight = this.MediaBox.Height;

            double llx = n1.Value;
            double lly = n2.Value;
            double urx = n3.Value;
            double ury = n4.Value;

            // Convertir de PDF [LLx LLy URx URy] a nuestro PdfRectangle (X, Y_TopLeft, Width, Height)
            double topLeftX = llx;
            // Y_TopLeft = AlturaTotal - URY_pdf
            double topLeftY = mediaBoxHeight - ury;
            double width = urx - llx;
            // Height = URY_pdf - LLY_pdf
            double height = ury - lly;

            if (width < 0 || height < 0)
            {
                Console.WriteLine($"Warning: Calculated negative width/height ({width}x{height}) when reading rectangle '{key.Value}'. PDF data might be invalid: [{llx} {lly} {urx} {ury}] with MediaBoxHeight {mediaBoxHeight}");
                // Decide cómo manejar esto: devolver null, Empty, o intentar corregir? Devolver null es más seguro.
                return null;
            }

            return new PdfRectangle(topLeftX, topLeftY, width, height);
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
