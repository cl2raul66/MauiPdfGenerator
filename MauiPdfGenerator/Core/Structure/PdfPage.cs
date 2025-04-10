using MauiPdfGenerator.Common.Geometry;
using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Content; // Para PdfResources

namespace MauiPdfGenerator.Core.Structure;

internal class PdfPage : PdfDictionary
{
    private readonly PdfDocument _document;
    private readonly PdfResources _pageResources; // Mover declaración aquí

    // --- Propiedades Requeridas ---
    public PdfReference Parent
    {
        get => this[PdfName.Parent] as PdfReference ?? throw new InvalidOperationException("Page /Parent reference is missing or invalid.");
        internal set
        {
            if (value is null) throw new ArgumentNullException(nameof(value), "/Parent entry cannot be null for a Page.");
            Add(PdfName.Parent, value);
        }
    }

    // --- Propiedad MediaBox (con conversión corregida) ---
    public PdfRectangle MediaBox
    {
        // Usa el getter que convierte DESDE PDF Array
        get => GetRectangleFromArray(this[PdfName.MediaBox] as PdfArray, true) // Indica que es MediaBox para referencia de altura
               ?? throw new InvalidOperationException("Page /MediaBox is missing or invalid.");
        // Usa el setter que convierte HACIA PDF Array
        set => Add(PdfName.MediaBox, CreatePdfArrayFromUiRect(value, value.Height)); // Usa la propia altura del rect como referencia
    }

    // --- Propiedad CropBox (con conversión corregida) ---
    public PdfRectangle? CropBox
    {
        // Usa el getter que convierte DESDE PDF Array, usando la altura del MediaBox como referencia
        get => GetRectangleFromArray(this[PdfName.CropBox] as PdfArray, false); // No es MediaBox
        // Usa el setter que convierte HACIA PDF Array, usando la altura del MediaBox como referencia
        set => AddOrRemove(PdfName.CropBox, value.HasValue ? CreatePdfArrayFromUiRect(value.Value, this.MediaBox.Height) : null);
    }


    public int Rotate // Sin cambios necesarios aquí
    {
        get => (int?)(this[PdfName.Rotate] as PdfNumber)?.Value ?? 0;
        set
        {
            if (value % 90 != 0) throw new ArgumentException("Rotate value must be a multiple of 90.", nameof(value));
            if (value == 0) Remove(PdfName.Rotate);
            else Add(PdfName.Rotate, new PdfNumber(value));
        }
    }

    internal PdfResources PageResources => _pageResources; // Getter para el manager/builder

    public PdfObject Contents // Sin cambios necesarios aquí
    {
        get => this[PdfName.Contents] ?? PdfNull.Instance; // Devolver Null si no existe
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value), "/Contents entry cannot be null.");
            Add(PdfName.Contents, value);
        }
    }

    // --- Constructor (Asegura orden correcto) ---
    internal PdfPage(PdfDocument document, PdfRectangle mediaBox, PdfReference parentRef) : base()
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));

        Add(PdfName.Type, PdfName.Page);
        Parent = parentRef;

        // !! Importante: Establecer MediaBox PRIMERO usando el método Add directo
        // con la conversión correcta que NO lee this.MediaBox aún.
        Add(PdfName.MediaBox, CreatePdfArrayFromUiRect(mediaBox, mediaBox.Height));

        // Ahora this.MediaBox es legible por otros métodos si es necesario (ej. al establecer CropBox)

        // Inicializar Resources
        _pageResources = new PdfResources(document);
        // Añadir referencia indirecta a Resources (PdfDocument se encargará de añadir _pageResources como indirecto)
        var resourcesRef = _document.GetReference(_pageResources); // Obtiene o crea indirecto
        Add(PdfName.Resources, resourcesRef); // Añade la REFERENCIA al diccionario de página


        // Contents se establecerá más tarde
    }


    // --- Helper Methods CORREGIDOS ---

    /// <summary>
    /// Crea un PdfArray [LLx LLy URx URy] desde un PdfRectangle (UI Coords, Top-Left).
    /// </summary>
    /// <param name="rectUi">Rectángulo en coordenadas UI (Origen Top-Left, Y hacia abajo).</param>
    /// <param name="referenceHeight">La altura total del espacio de coordenadas de referencia (ej. MediaBox.Height) para la conversión Y.</param>
    /// <returns>PdfArray en formato PDF.</returns>
    private static PdfArray CreatePdfArrayFromUiRect(PdfRectangle rectUi, double referenceHeight)
    {
        if (referenceHeight <= 0)
        {
            // Evitar división por cero o resultados extraños si la altura de referencia no es válida
            // Podría pasar si se llama antes de establecer MediaBox correctamente.
            // Considera lanzar una excepción o usar un valor por defecto seguro.
            Console.WriteLine($"Warning: Invalid referenceHeight ({referenceHeight}) in CreatePdfArrayFromUiRect. Using fallback.");
            referenceHeight = rectUi.Height > 0 ? rectUi.Height : 792; // Fallback a Letter height
        }

        double llx = rectUi.X;
        // LLY = AlturaRef - Y_UI_Top - AlturaRect = AlturaRef - rectUi.Bottom
        double lly = referenceHeight - rectUi.Bottom;
        double urx = rectUi.Right;
        // URY = AlturaRef - Y_UI_Top
        double ury = referenceHeight - rectUi.Top;

        // Asegurarse de que lly <= ury
        if (lly > ury) { (lly, ury) = (ury, lly); }

        return new PdfArray(
            new PdfNumber(llx),
            new PdfNumber(lly),
            new PdfNumber(urx),
            new PdfNumber(ury)
        );
    }

    /// <summary>
    /// Obtiene un PdfRectangle (UI Coords, Top-Left) desde un PdfArray [LLx LLy URx URy].
    /// </summary>
    /// <param name="pdfArray">El array PDF.</param>
    /// <param name="isMediaBox">Indica si este array representa el MediaBox (para saber qué altura usar como referencia).</param>
    /// <returns>PdfRectangle en coordenadas UI, o null si el array es inválido.</returns>
    private PdfRectangle? GetRectangleFromArray(PdfArray? pdfArray, bool isMediaBox)
    {
        if (pdfArray is null || pdfArray.Count != 4) return null;

        if (pdfArray.ElementAtOrDefault(0) is PdfNumber n1 && // LLx
            pdfArray.ElementAtOrDefault(1) is PdfNumber n2 && // LLy
            pdfArray.ElementAtOrDefault(2) is PdfNumber n3 && // URx
            pdfArray.ElementAtOrDefault(3) is PdfNumber n4)   // URy
        {
            double llx = n1.Value;
            double lly = n2.Value;
            double urx = n3.Value;
            double ury = n4.Value;

            // Determinar la altura de referencia para la conversión Y
            double referenceHeight;
            if (isMediaBox)
            {
                // Si estamos obteniendo el MediaBox, su altura PDF es URY - LLY
                referenceHeight = ury - lly;
                if (referenceHeight <= 0) return null; // MediaBox inválido
            }
            else
            {
                // Si obtenemos otro box (CropBox), la referencia es la altura del MediaBox existente
                referenceHeight = this.MediaBox.Height; // Ahora es seguro leer MediaBox
                if (referenceHeight <= 0) return null; // MediaBox base no es válido
            }

            // Convertir de PDF [LLx LLy URx URy] a nuestro PdfRectangle (X, Y_TopLeft, Width, Height)
            double topLeftX = llx;
            double topLeftY = referenceHeight - ury; // Y_UI = AlturaRef - URY_pdf
            double width = urx - llx;
            double height = ury - lly; // Height = URY_pdf - LLY_pdf

            if (width < 0 || height < 0) return null; // Dimensiones inválidas

            return new PdfRectangle(topLeftX, topLeftY, width, height);
        }
        return null;
    }

    // Helper AddOrRemove sin cambios, usa las propiedades que llaman a los métodos de conversión
    private void AddOrRemove(PdfName key, PdfObject? value)
    {
        if (value == null || value is PdfNull) Remove(key);
        else Add(key, value);
    }

} // Fin clase PdfPage
