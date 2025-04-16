using MauiPdfGenerator.Common.Elements;
using MauiPdfGenerator.Common.Primitives;

namespace MauiPdfGenerator.Common;

/// <summary>
/// Represents a single page within the PDF document, including its specific
/// settings and the elements it contains.
/// </summary>
internal class PageModel
{
    /// <summary>
    /// Specific size for this page. If Empty, use document default.
    /// Units are defined by DocumentSettings.Units.
    /// </summary>
    public PdfSize Size { get; set; } = PdfSize.Empty; // Indicates use document default

    /// <summary>
    /// Specific margins for this page. If null, use document default.
    /// Units are defined by DocumentSettings.Units.
    /// </summary>
    public PdfMargins? Margins { get; set; } = null; // Indicates use document default

    /// <summary>
    /// List of elements to be drawn on this page in order.
    /// </summary>
    public List<ElementModel> Elements { get; } // Readonly property, initialized in constructor

    public PageModel()
    {
        Elements = new List<ElementModel>();
    }
}
