using MauiPdfGenerator.Common.Primitives;

namespace MauiPdfGenerator.Common.Elements;

/// <summary>
/// Abstract base class for all elements that can be placed on a PDF page.
/// The Position typically represents the top-left corner or a reference point,
/// depending on the specific element type and its alignment properties.
/// </summary>
internal abstract class ElementModel
{
    /// <summary>
    /// The reference position of the element on the page.
    /// Units are defined by DocumentSettings.Units and converted to points by the Core.
    /// </summary>
    public PdfPoint Position { get; set; }
}
