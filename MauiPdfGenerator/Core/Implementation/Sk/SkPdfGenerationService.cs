using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Builders; 
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class SkPdfGenerationService : IPdfGenerationService
{
    private TextRenderer _textRenderer = new(); 
    private ImageRenderer _imageRenderer = new(); 
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
                // bool isFirstPhysicalPageForThisLogicalPage = true; // Descomentar si se necesita

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
                        if (currentProcessingElement is null && elementsToProcess.Count == 0) break;
                        continue;
                    }

                    float currentY = currentPageContentRect.Top;
                    bool pageHasDrawnContent = false;

                    while (currentProcessingElement != null || elementsToProcess.Count > 0)
                    {
                        PdfElement elementToRender;
                        // bool elementIsBeingRetriedOnNewPage; // Descomentar si se necesita

                        if (currentProcessingElement != null)
                        {
                            elementToRender = currentProcessingElement;
                            // elementIsBeingRetriedOnNewPage = true;
                        }
                        else
                        {
                            elementToRender = elementsToProcess.Dequeue();
                            // elementIsBeingRetriedOnNewPage = false;
                        }

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
                                result = await _textRenderer.RenderAsync(canvas, para, originalPageDefinition, currentPageContentRect, currentY, _currentFontRegistry);
                                break;
                            case PdfImage img:
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

                        if (currentProcessingElement != null)
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

                        if (elementsToProcess.Count == 0 && currentProcessingElement is null)
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
                        // No content was drawn on this page, and no elements remain for this logical page
                        // This can happen if the very first element of a logical page doesn't fit and is moved.
                        // Don't call EndPage if BeginPage was the last call.
                        // However, the outer loop condition (elementsToProcess.Count > 0 || currentProcessingElement != null)
                        // should prevent an empty page from being added if nothing was ever going to be drawn.
                        // If BeginPage was called, EndPage must be called.
                        pdfDoc.EndPage();
                    }
                    // isFirstPhysicalPageForThisLogicalPage = false; // Descomentar si se necesita
                }
            }

            pdfDoc.Close();
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR SkPdfGenerationService: {ex}");
            throw new PdfGenerationException($"An unexpected error occurred during PDF generation: {ex.Message}", ex);
        }
        finally
        {
            _currentFontRegistry = null; 
        }
    }

    private float RenderHorizontalLine(SKCanvas canvas, PdfHorizontalLine line, PdfPageData pageData, SKRect contentRect, float currentY)
    {
        float thickness = line.CurrentThickness > 0 ? line.CurrentThickness : PdfHorizontalLine.DefaultThickness;
        Microsoft.Maui.Graphics.Color color = line.CurrentColor ?? PdfHorizontalLine.DefaultColor; 
        if (thickness <= 0) thickness = PdfHorizontalLine.DefaultThickness;

        float lineContentX = contentRect.Left + (float)line.GetMargin.Left;
        float lineContentWidth = contentRect.Width - (float)line.GetMargin.Left - (float)line.GetMargin.Right;

        if (lineContentWidth <= 0) return thickness;

        float availableHeightForLineContent = contentRect.Bottom - currentY - (float)line.GetMargin.Bottom;

        if (thickness > availableHeightForLineContent)
        {
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
        float lineDrawY = currentY + thickness / 2f;

        if (lineDrawY - thickness / 2f < contentRect.Top - 0.01f || lineDrawY + thickness / 2f > contentRect.Bottom + 0.01f)
        {
            return thickness;
        }

        canvas.DrawLine(startX, lineDrawY, endX, lineDrawY, paint);
        return thickness;
    }
}
