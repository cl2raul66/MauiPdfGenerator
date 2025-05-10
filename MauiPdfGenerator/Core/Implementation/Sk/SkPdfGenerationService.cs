using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class SkPdfGenerationService : IPdfGenerationService
{
    private readonly TextRenderer _textRenderer;
    private readonly ImageRenderer _imageRenderer;

    public SkPdfGenerationService()
    {
        _textRenderer = new TextRenderer();
        _imageRenderer = new ImageRenderer();
    }

    public async Task GenerateAsync(PdfDocumentData documentData, string filePath)
    {
        try
        {
            var metadata = new SKDocumentPdfMetadata
            {
                Title = documentData.Title ?? string.Empty,
                Author = documentData.Author ?? string.Empty,
                Subject = documentData.Subject ?? string.Empty,
                Keywords = documentData.Keywords ?? string.Empty,
                Creator = documentData.Creator ?? string.Empty,
                Producer = documentData.Producer ?? "MauiPdfGenerator (SkiaSharp)",
                Creation = documentData.CreationDate ?? DateTime.Now,
                Modified = DateTime.Now,
                RasterDpi = 300,
                EncodingQuality = 100,
                PdfA = false
            };

            using var stream = new SKFileWStream(filePath);
            using var pdfDoc = SKDocument.CreatePdf(stream, metadata) ?? throw new PdfGenerationException("SkiaSharp failed to create the PDF document stream.");

            foreach (var originalPageDefinition in documentData.Pages)
            {
                var elementsToProcess = new Queue<PdfElement>(originalPageDefinition.Elements);
                PdfElement? currentProcessingElement = null;
                bool isFirstPhysicalPageForThisLogicalPage = true;

                while (elementsToProcess.Count > 0 || currentProcessingElement != null)
                {
                    SKSize pageSize = SkiaUtils.GetSkPageSize(originalPageDefinition.Size, originalPageDefinition.Orientation);
                    using SKCanvas canvas = pdfDoc.BeginPage(pageSize.Width, pageSize.Height);

                    canvas.Clear(originalPageDefinition.BackgroundColor is not null
                        ? SkiaUtils.ConvertToSkColor(originalPageDefinition.BackgroundColor)
                        : SKColors.White);

                    var pageMargins = originalPageDefinition.Margins;
                    var currentPageContentRect = new SKRect(
                        (float)pageMargins.Left,
                        (float)pageMargins.Top,
                        pageSize.Width - (float)pageMargins.Right,
                        pageSize.Height - (float)pageMargins.Bottom
                    );

                    if (currentPageContentRect.Width <= 0 || currentPageContentRect.Height <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Page contentRect for page definition is zero or negative. Page Size: {pageSize}, Margins: {pageMargins}. Skipping physical page.");
                        pdfDoc.EndPage();
                        if (currentProcessingElement == null && elementsToProcess.Count == 0) break;
                        continue;
                    }

                    float currentY = currentPageContentRect.Top;
                    bool pageHasDrawnContent = false;

                    while (currentProcessingElement != null || elementsToProcess.Count > 0)
                    {
                        PdfElement elementToRender;
                        bool elementIsBeingRetriedOnNewPage;

                        if (currentProcessingElement != null)
                        {
                            elementToRender = currentProcessingElement;
                            elementIsBeingRetriedOnNewPage = true;
                        }
                        else
                        {
                            elementToRender = elementsToProcess.Dequeue();
                            elementIsBeingRetriedOnNewPage = false;
                        }

                        bool isElementIntrinsicallyAContinuation = (elementToRender is PdfParagraph p && p.IsContinuation);

                        float elementTopMarginToApply = (float)elementToRender.GetMargin.Top;

                        // Do not apply top margin if:
                        // 1. The element is intrinsically a continuation (e.g., split paragraph).
                        // OR
                        // 2. The element is an *original* element (not intrinsically a continuation)
                        //    that's being retried on a new page *AND* it's not the very first element
                        //    being placed on this *physical* page (i.e., currentY is still at page top).
                        //    This is nuanced: if an image moves to a new page, it *should* get its top margin.
                        //    If a paragraph that was *not* split (IsContinuation=false) moves to a new page,
                        //    it also should get its top margin.
                        // The key is `isElementIntrinsicallyAContinuation`.
                        // A whole element moved to a new page (elementIsBeingRetriedOnNewPage=true but IsContinuation=false)
                        // *should* get its top margin.
                        if (isElementIntrinsicallyAContinuation)
                        {
                            elementTopMarginToApply = 0;
                        }

                        // Check for overflow from top margin application
                        if (currentY + elementTopMarginToApply > currentPageContentRect.Bottom + 0.01f)
                        {
                            // If currentY is still at the page top, and even the margin doesn't fit, this element is problematic
                            // or the page is too small. For now, assume we break and try on a new (hopefully larger) page.
                            if (currentY == currentPageContentRect.Top)
                            {
                                System.Diagnostics.Debug.WriteLine($"Warning: Element '{elementToRender.GetType().Name}' top margin ({elementTopMarginToApply}) alone exceeds available page height. CurrentY: {currentY}, Bottom: {currentPageContentRect.Bottom}");
                            }
                            currentProcessingElement = elementToRender;
                            break;
                        }
                        currentY += elementTopMarginToApply;

                        RenderOutput result;
                        switch (elementToRender)
                        {
                            case PdfParagraph para:
                                // Call to TextRenderer.Render (no extra boolean)
                                result = _textRenderer.Render(canvas, para, originalPageDefinition, currentPageContentRect, currentY);
                                break;
                            case PdfImage img:
                                // Call to ImageRenderer.RenderAsync (no extra boolean)
                                result = await _imageRenderer.RenderAsync(canvas, img, originalPageDefinition, currentPageContentRect, currentY);
                                break;
                            case PdfHorizontalLine line:
                                float hlHeight = RenderHorizontalLine(canvas, line, originalPageDefinition, currentPageContentRect, currentY);
                                result = new RenderOutput(hlHeight, null, false);
                                break;
                            default:
                                result = new RenderOutput(0, null, false);
                                break;
                        }

                        // After rendering, currentProcessingElement should be the remnant, if any.
                        // It will be null if elementToRender was fully processed or if a placeholder was drawn for it.
                        currentProcessingElement = result.RemainingElement;

                        if (result.HeightDrawnThisCall > 0)
                        {
                            pageHasDrawnContent = true;
                        }
                        currentY += result.HeightDrawnThisCall;

                        if (result.RequiresNewPage)
                        {
                            // If HeightDrawnThisCall is 0, it implies the elementToRender (or its current state if it was already a remnant)
                            // was not drawn and needs to move entirely. `result.RemainingElement` should be this element.
                            // currentProcessingElement is already set from result.RemainingElement.
                            // If HeightDrawnThisCall > 0, part was drawn, and the remnant is in currentProcessingElement.
                            // In both cases, break to start a new physical page.
                            break;
                        }

                        // If there's a remnant (currentProcessingElement is not null after render), it means the element was split
                        // and the remainder needs to go to the next page.
                        if (currentProcessingElement != null)
                        {
                            break;
                        }

                        // --- Element (or its current part) fully rendered on this page ---
                        float elementBottomMargin = (float)elementToRender.GetMargin.Bottom;
                        if (currentY + elementBottomMargin > currentPageContentRect.Bottom + 0.01f)
                        {
                            // Not enough space for bottom margin. Element is done.
                            // Next element (if any) will start on a new page.
                            break;
                        }
                        currentY += elementBottomMargin;

                        bool isLastElementForThisLogicalDefinition = (elementsToProcess.Count == 0 && currentProcessingElement == null);
                        if (!isLastElementForThisLogicalDefinition)
                        {
                            if (currentY + originalPageDefinition.PageDefaultSpacing > currentPageContentRect.Bottom + 0.01f)
                            {
                                break;
                            }
                            currentY += originalPageDefinition.PageDefaultSpacing;
                        }

                        if (currentY >= currentPageContentRect.Bottom + 0.01f)
                        {
                            break;
                        }

                        if (elementsToProcess.Count == 0 && currentProcessingElement == null)
                        {
                            break;
                        }
                    }

                    pdfDoc.EndPage();
                    isFirstPhysicalPageForThisLogicalPage = false;

                }
            }

            pdfDoc.Close();
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR SkPdfGenerationService: {ex}");
            throw new PdfGenerationException($"An unexpected error occurred during PDF generation: {ex.Message}", ex);
        }
    }

    private float RenderHorizontalLine(SKCanvas canvas, PdfHorizontalLine line, PdfPageData pageData, SKRect contentRect, float currentY)
    {
        float thickness = line.CurrentThickness > 0 ? line.CurrentThickness : PdfHorizontalLine.DefaultThickness;
        Color color = line.CurrentColor ?? PdfHorizontalLine.DefaultColor;
        if (thickness <= 0) thickness = PdfHorizontalLine.DefaultThickness;

        float lineContentX = contentRect.Left + (float)line.GetMargin.Left;
        float lineContentWidth = contentRect.Width - (float)line.GetMargin.Left - (float)line.GetMargin.Right;

        if (lineContentWidth <= 0) return thickness;

        float availableHeightForLineContent = contentRect.Bottom - currentY - (float)line.GetMargin.Bottom;

        // An HL's "content" is its thickness. Check if this fits.
        if (thickness > availableHeightForLineContent)
        {
            // Not enough vertical space for the line itself.
            // Per spec, HL does not paginate. Return its thickness for layout advancement, but don't draw.
            return thickness;
        }

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SkiaUtils.ConvertToSkColor(color),
            StrokeWidth = thickness,
            IsAntialias = true
        };

        float startX = lineContentX;
        float endX = lineContentX + lineContentWidth;
        // Line is drawn at currentY (which is after its top margin), centered on its thickness
        float lineDrawY = currentY + thickness / 2f;

        // Final check to ensure the drawing coordinates are sane (though availableHeightForLineContent should cover Y)
        if (lineDrawY - thickness / 2f < contentRect.Top - 0.1f || lineDrawY + thickness / 2f > contentRect.Bottom + 0.1f)
        {
            return thickness; // Should be caught by availableHeightForLineContent check
        }

        canvas.DrawLine(startX, lineDrawY, endX, lineDrawY, paint);
        return thickness; // The height consumed by the line is its thickness
    }
}
