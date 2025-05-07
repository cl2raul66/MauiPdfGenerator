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

            for (int pageIndex = 0; pageIndex < documentData.Pages.Count; pageIndex++)
            {
                var pageData = documentData.Pages[pageIndex];
                SKSize pageSize = SkiaUtils.GetSkPageSize(pageData.Size, pageData.Orientation);
                using SKCanvas canvas = pdfDoc.BeginPage(pageSize.Width, pageSize.Height);

                canvas.Clear(pageData.BackgroundColor is not null ? SkiaUtils.ConvertToSkColor(pageData.BackgroundColor) : SKColors.White);

                var margins = pageData.Margins;
                var contentRect = new SKRect(
                    (float)margins.Left,
                    (float)margins.Top,
                    pageSize.Width - (float)margins.Right,
                    pageSize.Height - (float)margins.Bottom
                );

                if (contentRect.Width <= 0 || contentRect.Height <= 0)
                {
                    pdfDoc.EndPage();
                    continue;
                }

                float currentY = contentRect.Top;

                foreach (var element in pageData.Elements)
                {
                    string imageSourceInfo = "";
                    if (element is PdfImage pdfImgElement)
                    {
                        imageSourceInfo = $"(Stream HashCode: {pdfImgElement.ImageStream.GetHashCode()})";
                    }

                    if (currentY + (float)element.GetMargin.Top >= contentRect.Bottom + 0.01f)
                    {
                        break;
                    }

                    currentY += (float)element.GetMargin.Top;

                    float elementRenderedHeight = 0;

                    switch (element)
                    {
                        case PdfParagraph p:
                            System.Diagnostics.Debug.WriteLine($"Rendering Paragraph: '{p.Text[..Math.Min(p.Text.Length, 20)]}'...");
                            elementRenderedHeight = _textRenderer.Render(canvas, p, pageData, contentRect, currentY);
                            break;
                        case PdfImage i:
                            elementRenderedHeight = await _imageRenderer.RenderAsync(canvas, i, pageData, contentRect, currentY);
                            break;
                        case PdfHorizontalLine l:
                            elementRenderedHeight = RenderHorizontalLine(canvas, l, pageData, contentRect, currentY);
                            break;
                        default:
                            elementRenderedHeight = 0;
                            break;
                    }

                    currentY += elementRenderedHeight;

                    if (currentY > contentRect.Bottom + 0.1f && elementRenderedHeight > 0)
                    {
                        currentY = contentRect.Bottom + 1;
                        break;
                    }

                    currentY += (float)element.GetMargin.Bottom;

                    if (currentY > contentRect.Bottom + 0.1f)
                    {
                        currentY = contentRect.Bottom + 1;
                        break;
                    }

                    if (element != pageData.Elements[^1])
                    {
                        currentY += pageData.PageDefaultSpacing;
                        if (currentY > contentRect.Bottom + 0.1f)
                        {
                            currentY = contentRect.Bottom + 1;
                            break;
                        }
                    }
                }
                pdfDoc.EndPage();
            }
            pdfDoc.Close();
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            throw new PdfGenerationException($"An unexpected error occurred during PDF generation: {ex.Message}", ex);
        }
    }

    private float RenderHorizontalLine(SKCanvas canvas, PdfHorizontalLine line, PdfPageData pageData, SKRect contentRect, float currentY)
    {
        float thickness = line.CurrentThickness > 0 ? line.CurrentThickness : PdfHorizontalLine.DefaultThickness;
        Color color = line.CurrentColor ?? PdfHorizontalLine.DefaultColor;

        if (thickness <= 0) thickness = PdfHorizontalLine.DefaultThickness;

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SkiaUtils.ConvertToSkColor(color),
            StrokeWidth = thickness,
            IsAntialias = true
        };

        float startX = contentRect.Left + (float)line.GetMargin.Left;
        float endX = contentRect.Right - (float)line.GetMargin.Right;
        float lineYpos = currentY + thickness / 2f;

        if (startX >= endX)
        {
            return thickness;
        }

        if (lineYpos > contentRect.Bottom + 0.1f || lineYpos < contentRect.Top - 0.1f)
        {
            return thickness;
        }

        canvas.DrawLine(startX, lineYpos, endX, lineYpos, paint);
        return thickness;
    }
}
