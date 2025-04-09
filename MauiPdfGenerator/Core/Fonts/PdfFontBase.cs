using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Structure; // For PdfDocument access maybe

namespace MauiPdfGenerator.Core.Fonts;

/// <summary>
/// Base class or interface for PDF font representations.
/// Responsible for generating the corresponding font dictionary object.
/// </summary>
internal abstract class PdfFontBase
{
    // Common properties might include Font Name, Metrics?

    /// <summary>
    /// Gets or creates the PdfObject (typically a PdfDictionary) that represents
    /// this font in the PDF document structure. This object will be added indirectly.
    /// </summary>
    /// <param name="document">The document context, needed for adding related objects (like font descriptors or files).</param>
    /// <returns>The PdfObject representing the font dictionary.</returns>
    internal abstract PdfObject GetPdfObject(PdfDocument document);

    /// <summary>
    /// Codifica el texto usando la codificación apropiada para la fuente.
    /// </summary>
    /// <param name="text">El texto a codificar</param>
    /// <returns>Los bytes codificados según la codificación de la fuente</returns>
    internal abstract byte[] EncodeText(string text);

    /// <summary>
    /// Obtiene el ancho aproximado del texto renderizado con esta fuente y tamaño.
    /// La unidad es puntos PDF (1/72 pulgadas).
    /// </summary>
    /// <param name="text">El texto a medir.</param>
    /// <param name="fontSize">El tamaño de la fuente en puntos.</param>
    /// <returns>El ancho calculado del texto.</returns>
    internal abstract double GetTextWidth(string text, double fontSize);
}
