using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Implementation.Sk.Pages;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Listeners;
using MauiPdfGenerator.Fluent.Builders;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using MauiPdfGenerator.Core.Implementation.Sk.Utils;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class SkComposer : IPdfCoreGenerator
{
    private readonly PageRendererFactory _pageRendererFactory = new();
    private readonly IElementRendererFactory _elementRendererFactory = new ElementRendererFactory();
    private readonly ILogger<SkComposer> _logger;
    private readonly IDiagnosticSink _diagnosticSink;
    private readonly IVisualDiagnosticStore? _visualStore;
    private readonly IDiagnosticVisualizer? _visualizer;

    public SkComposer(ILogger<SkComposer> logger, IDiagnosticSink diagnosticSink, IVisualDiagnosticStore? visualStore = null, IDiagnosticVisualizer? visualizer = null)
    {
        _logger = logger;
        _diagnosticSink = diagnosticSink;
        _visualStore = visualStore;
        _visualizer = visualizer;
    }

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

            var layoutState = new Dictionary<object, object>();

            for (int i = 0; i < documentData.Pages.Count; i++)
            {
                var pageDefinition = documentData.Pages[i];
                _logger.LogDebug("Processing Page Definition {PageIndex} of type {PageType}", i + 1, pageDefinition.GetType().Name);

                var context = new PdfGenerationContext(pageDefinition, fontRegistry, layoutState, _logger, _elementRendererFactory, _diagnosticSink);
                IPageRenderer pageRenderer = _pageRendererFactory.GetRenderer(pageDefinition);

                var pageBlocks = await pageRenderer.LayoutAsync(context);

                _logger.LogDebug("Page Definition {PageIndex} resulted in {PageCount} physical page(s).", i + 1, pageBlocks.Count);

                foreach (var block in pageBlocks)
                {
                    SKSize pageSize = SkiaUtils.GetSkPageSize(pageDefinition.Size, pageDefinition.Orientation);
                    using var canvas = pdfDoc.BeginPage(pageSize.Width, pageSize.Height);

                    canvas.Clear(pageDefinition.BackgroundColor is not null ? SkiaUtils.ConvertToSkColor(pageDefinition.BackgroundColor) : SKColors.White);

                    await pageRenderer.RenderPageBlockAsync(canvas, block, context);

                    RenderDiagnosticsOverlay(canvas);

                    pdfDoc.EndPage();
                }
            }

            pdfDoc.Close();
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            _logger.LogError(ex, "An unexpected error occurred during PDF generation.");
            throw new PdfGenerationException($"An unexpected error occurred during PDF generation: {ex.Message}", ex);
        }
    }

    private void RenderDiagnosticsOverlay(SKCanvas canvas)
    {
        if (_visualizer is null || _visualStore is null)
        {
            return;
        }

        var messagesToDraw = _visualStore.GetPendingMessages();
        if (!messagesToDraw.Any())
        {
            return;
        }

        var canvasAdapter = new SkiaDiagnosticCanvasAdapter(canvas);

        foreach (var message in messagesToDraw)
        {
            if (_visualizer.CanVisualize(message))
            {
                _visualizer.Visualize(canvasAdapter, message);
            }
        }

        _visualStore.ClearPendingMessages();
    }
}
