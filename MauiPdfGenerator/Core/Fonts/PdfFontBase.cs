using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Structure; // For PdfDocument access maybe

namespace MauiPdfGenerator.Core.Fonts
{
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

        // TODO: Add methods for encoding text, getting character widths etc.
        // internal abstract byte[] EncodeText(string text);
        // internal abstract double GetTextWidth(string text, double fontSize);
    }
}
