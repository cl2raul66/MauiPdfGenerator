using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Builders; 
using SkiaSharp;
using System.Diagnostics;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class SkPdfGenerationService : IPdfGenerationService
{
    private readonly TextRenderer _textRenderer = new();
    private readonly ImageRenderer _imageRenderer = new();
    private readonly LayoutRenderer _layoutRenderer = new();
    private readonly GridRenderer _gridRenderer = new();
    private PdfFontRegistryBuilder? _currentFontRegistry;

    public async Task GenerateAsync(PdfDocumentData documentData, string filePath, PdfFontRegistryBuilder fontRegistry)
    {
        _currentFontRegistry = fontRegistry;

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

                while (elementsToProcess.Count > 0 || currentProcessingElement is not null)
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
                        Debug.WriteLine($"Warning: Page contentRect for page definition is zero or negative. Page Size: {pageSize}, Margins: {pageMargins}. Skipping physical page.");
                        pdfDoc.EndPage();
                        if (currentProcessingElement is null && elementsToProcess.Count == 0) break;
                        continue;
                    }

                    float currentY = currentPageContentRect.Top;
                    bool pageHasDrawnContent = false;

                    while (currentProcessingElement is not null || elementsToProcess.Count > 0)
                    {
                        PdfElement elementToRender = currentProcessingElement ?? elementsToProcess.Dequeue();
                        currentProcessingElement = null;

                        bool isElementIntrinsicallyAContinuation = (elementToRender is PdfParagraph p && p.IsContinuation);
                        float elementTopMarginToApply = (float)elementToRender.GetMargin.Top;

                        if (isElementIntrinsicallyAContinuation)
                        {
                            elementTopMarginToApply = 0;
                        }

                        if (currentY + elementTopMarginToApply > currentPageContentRect.Bottom + 0.01f)
                        {
                            if (currentY == currentPageContentRect.Top)
                            {
                                Debug.WriteLine($"Warning: Element '{elementToRender.GetType().Name}' top margin ({elementTopMarginToApply}) alone exceeds available page height. CurrentY: {currentY}, Bottom: {currentPageContentRect.Bottom}");
                            }
                            currentProcessingElement = elementToRender;
                            break;
                        }
                        currentY += elementTopMarginToApply;

                        var result = await RenderElementAsync(canvas, elementToRender, originalPageDefinition, currentPageContentRect, currentY);

                        currentProcessingElement = result.RemainingElement;

                        if (result.HeightDrawnThisCall > 0)
                        {
                            pageHasDrawnContent = true;
                        }
                        currentY += result.HeightDrawnThisCall;

                        if (result.RequiresNewPage)
                        {
                            break;
                        }

                        if (currentProcessingElement is not null)
                        {
                            break;
                        }

                        float elementBottomMargin = (float)elementToRender.GetMargin.Bottom;
                        if (currentY + elementBottomMargin > currentPageContentRect.Bottom + 0.01f)
                        {
                            break;
                        }
                        currentY += elementBottomMargin;

                        bool isLastElementForThisLogicalDefinition = (elementsToProcess.Count == 0 && currentProcessingElement is null);
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
                    }
                    if (pageHasDrawnContent || currentProcessingElement is not null || elementsToProcess.Any())
                    {
                        pdfDoc.EndPage();
                    }
                    else if (!pageHasDrawnContent && elementsToProcess.Count == 0 && currentProcessingElement is null)
                    {
                        pdfDoc.EndPage();
                    }
                }
            }

            pdfDoc.Close();
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            Debug.WriteLine($"ERROR SkPdfGenerationService: {ex}");
            throw new PdfGenerationException($"An unexpected error occurred during PDF generation: {ex.Message}", ex);
        }
        finally
        {
            _currentFontRegistry = null;
        }
    }

    private async Task<RenderOutput> RenderElementAsync(SKCanvas canvas, PdfElement element, PdfPageData pageDef, SKRect availableRect, float currentY)
    {
        return element switch
        {
            PdfParagraph para => await _textRenderer.RenderAsync(canvas, para, pageDef, availableRect, currentY, _currentFontRegistry),
            PdfImage img => await _imageRenderer.RenderAsync(canvas, img, pageDef, availableRect, currentY),
            PdfHorizontalLine line => RenderHorizontalLine(canvas, line, availableRect, currentY),
            PdfGrid grid => await _gridRenderer.RenderAsync(canvas, grid, pageDef, availableRect, currentY, RenderElementAsync),
            PdfVerticalStackLayout vsl => await _layoutRenderer.RenderVerticalStackLayoutAsync(canvas, vsl, pageDef, availableRect, currentY, RenderElementAsync),
            PdfHorizontalStackLayout hsl => await _layoutRenderer.RenderHorizontalStackLayoutAsync(canvas, hsl, pageDef, availableRect, currentY, RenderElementAsync),
            _ => new RenderOutput(0, 0, null, false),
        };
    }

    private RenderOutput RenderHorizontalLine(SKCanvas canvas, PdfHorizontalLine line, SKRect contentRect, float currentY)
    {
        float thickness = line.CurrentThickness > 0 ? line.CurrentThickness : PdfHorizontalLine.DefaultThickness;
        Color color = line.CurrentColor ?? PdfHorizontalLine.DefaultColor;
        if (thickness <= 0) thickness = PdfHorizontalLine.DefaultThickness;

        float lineContentX = contentRect.Left + (float)line.GetMargin.Left;
        float lineContentWidth = contentRect.Width - (float)line.GetMargin.Left - (float)line.GetMargin.Right;

        if (lineContentWidth <= 0) return new RenderOutput(thickness, 0, null, false);

        float availableHeightForLineContent = contentRect.Bottom - currentY - (float)line.GetMargin.Bottom;

        if (thickness > availableHeightForLineContent)
        {
            return new RenderOutput(thickness, lineContentWidth, null, true);
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
        float lineDrawY = currentY + thickness / 2f;

        if (lineDrawY - thickness / 2f < contentRect.Top - 0.01f || lineDrawY + thickness / 2f > contentRect.Bottom + 0.01f)
        {
            return new RenderOutput(thickness, lineContentWidth, null, true);
        }

        canvas.DrawLine(startX, lineDrawY, endX, lineDrawY, paint);
        return new RenderOutput(thickness, lineContentWidth, null, false);
    }
}
