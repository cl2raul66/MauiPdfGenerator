using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Implementation.Sk.Pages;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class SkComposer : IPdfCoreGenerator
{
    private readonly PageRendererFactory _rendererFactory = new();

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

            var layoutState = new Dictionary<PdfElement, object>();

            foreach (var pageDefinition in documentData.Pages)
            {
                IPageRenderer pageRenderer = _rendererFactory.GetRenderer(pageDefinition);

                var pageBlocks = await pageRenderer.LayoutAsync(pageDefinition, fontRegistry, layoutState);

                foreach (var block in pageBlocks)
                {
                    SKSize pageSize = SkiaUtils.GetSkPageSize(pageDefinition.Size, pageDefinition.Orientation);
                    using var canvas = pdfDoc.BeginPage(pageSize.Width, pageSize.Height);

                    await pageRenderer.RenderPageBlockAsync(canvas, pageDefinition, block, fontRegistry, layoutState);

                    pdfDoc.EndPage();
                }
            }

            pdfDoc.Close();
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            throw new PdfGenerationException($"An unexpected error occurred during PDF generation: {ex.Message}", ex);
        }
    }
}