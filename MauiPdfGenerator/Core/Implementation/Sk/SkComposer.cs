using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Implementation.Sk.Elements;
using MauiPdfGenerator.Core.Implementation.Sk.Pages;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class SkComposer : IPdfCoreGenerator
{
    private readonly PageRendererFactory _pageRendererFactory = new();
    private readonly ElementRendererFactory _elementRendererFactory = new();

    public async Task GenerateAsync(PdfDocumentData documentData, string filePath, PdfFontRegistryBuilder fontRegistry, ILogger logger)
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

            var layoutState = new Dictionary<object, object>();

            for (int i = 0; i < documentData.Pages.Count; i++)
            {
                var pageDefinition = documentData.Pages[i];
                logger.LogDebug("Processing Page {PageIndex} of type {PageType}", i + 1, pageDefinition.GetType().Name);

                var context = new PdfGenerationContext(pageDefinition, fontRegistry, layoutState, logger, _elementRendererFactory);
                IPageRenderer pageRenderer = _pageRendererFactory.GetRenderer(pageDefinition);

                var pageBlocks = await pageRenderer.LayoutAsync(context);

                foreach (var block in pageBlocks)
                {
                    SKSize pageSize = SkiaUtils.GetSkPageSize(pageDefinition.Size, pageDefinition.Orientation);
                    using var canvas = pdfDoc.BeginPage(pageSize.Width, pageSize.Height);

                    // Pintar fondo de página
                    canvas.Clear(pageDefinition.BackgroundColor is not null ? SkiaUtils.ConvertToSkColor(pageDefinition.BackgroundColor) : SKColors.White);

                    await pageRenderer.RenderPageBlockAsync(canvas, block, context);

                    pdfDoc.EndPage();
                }
            }

            pdfDoc.Close();
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            logger.LogError(ex, "An unexpected error occurred during PDF generation.");
            throw new PdfGenerationException($"An unexpected error occurred during PDF generation: {ex.Message}", ex);
        }
    }
}
