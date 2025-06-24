using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Builders;
using SkiaSharp;
using System.Diagnostics;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class SkPdfGenerationService : IPdfGenerationService
{
    private readonly ElementsRender _renderElements = new();

    public async Task GenerateAsync(PdfDocumentData documentData, string filePath, PdfFontRegistryBuilder fontRegistry)
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
                SKSize pageSize = SkiaUtils.GetSkPageSize(originalPageDefinition.Size, originalPageDefinition.Orientation);
                var pageMargins = originalPageDefinition.Margins;
                var contentRect = new SKRect(
                    (float)pageMargins.Left,
                    (float)pageMargins.Top,
                    pageSize.Width - (float)pageMargins.Right,
                    pageSize.Height - (float)pageMargins.Bottom
                );
                using var canvas = pdfDoc.BeginPage(pageSize.Width, pageSize.Height);
                canvas.Clear(originalPageDefinition.BackgroundColor is not null
                    ? SkiaUtils.ConvertToSkColor(originalPageDefinition.BackgroundColor)
                    : SKColors.White);
                await _renderElements.RenderPageAuto(canvas, originalPageDefinition, contentRect, fontRegistry);
                pdfDoc.EndPage();
            }

            pdfDoc.Close();
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            throw new PdfGenerationException($"An unexpected error occurred during PDF generation: {ex.Message}", ex);
        }
    }
}
