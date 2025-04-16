// --- START OF FILE MauiPdfGenerator/Core/PageRenderer.cs ---
using SkiaSharp;
using MauiPdfGenerator.Common.Elements;
using MauiPdfGenerator.Core.Rendering;
using MauiPdfGenerator.Core.Utils;
using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Core;

/// <summary>
/// Responsible for rendering the elements of a single PageModel onto an SKCanvas.
/// Assumes element coordinates and sizes are already in points.
/// </summary>
internal class PageRenderer
{
    // No longer needs UnitConverter passed down for element rendering
    public void RenderPage(SKCanvas canvas, PageModel pageModel, PdfFontManager fontManager, UnitConverter unitConverter) // UnitConverter still needed for MARGINS
    {
        if (canvas == null) throw new ArgumentNullException(nameof(canvas));
        if (pageModel == null) throw new ArgumentNullException(nameof(pageModel));
        if (fontManager == null) throw new ArgumentNullException(nameof(fontManager));
        if (unitConverter == null) throw new ArgumentNullException(nameof(unitConverter));

        bool marginsApplied = false;
        // Optional: Handle Margins - Requires access to DocumentSettings or passing resolved margins
        // Assuming DocumentModel is NOT accessible here directly. Margins need pre-calculation.
        // Let's assume margins are NOT handled here for now to simplify.
        // If they WERE handled, unitConverter WOULD be needed here for the margin values.

        // Render elements
        foreach (var element in pageModel.Elements)
        {
            if (element == null) continue;

            try
            {
                switch (element)
                {
                    case TextElementModel textElement:
                        // Pass only needed dependencies (no UnitConverter needed for coords)
                        TextRenderer.Render(canvas, textElement, fontManager);
                        break;

                    case ImageElementModel imageElement:
                        // No UnitConverter needed for rect
                        ImageRenderer.Render(canvas, imageElement);
                        break;

                    case LineElementModel lineElement:
                        // No UnitConverter needed for coords (thickness assumed points)
                        ShapeRenderer.RenderLine(canvas, lineElement);
                        break;

                    case RectangleElementModel rectElement:
                        // No UnitConverter needed for coords/size (thickness assumed points)
                        ShapeRenderer.RenderRectangle(canvas, rectElement);
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"Warning: Skipping unsupported element type: {element.GetType().FullName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rendering element {element.GetType().Name}: {ex.Message}");
            }
        }

        // Optional: Restore canvas if margins were handled
        // if (marginsApplied) canvas.Restore();
    }
}
// --- END OF FILE MauiPdfGenerator/Core/PageRenderer.cs ---
