using SkiaSharp;
using MauiPdfGenerator.Common.Primitives;
using MauiPdfGenerator.Core.Utils;
using MauiPdfGenerator.Common; // Alias for clarity

namespace MauiPdfGenerator.Core;

/// <summary>
/// Orchestrates the PDF generation process using SkiaSharp based on a DocumentModel.
/// </summary>
internal class PdfGenerationEngine
{
    /// <summary>
    /// Generates a PDF document based on the provided model and writes it to the output stream.
    /// </summary>
    /// <param name="documentModel">The internal representation of the document to generate.</param>
    /// <param name="outputStream">The stream to write the generated PDF to.</param>
    /// <returns>A task representing the asynchronous generation process.</returns>
    public async Task GeneratePdfAsync(DocumentModel documentModel, Stream outputStream)
    {
        ArgumentNullException.ThrowIfNull(documentModel);
        ArgumentNullException.ThrowIfNull(outputStream);
        if (!outputStream.CanWrite) throw new ArgumentException("Output stream is not writable.", nameof(outputStream));

        // Use Task.Run to avoid blocking the UI thread for potentially long operations
        await Task.Run(() =>
        {
            // Initialize utilities
            var unitConverter = new UnitConverter(documentModel.Settings.Units);
            using var fontManager = new PdfFontManager(); // Manages SKTypeface instances
            var pageRenderer = new PageRenderer(); // Renders elements onto a page canvas

            // Prepare PDF metadata for SkiaSharp
            var metadata = new SKDocumentPdfMetadata
            {
                Author = documentModel.Settings.Author,
                Title = documentModel.Settings.Title,
                Subject = documentModel.Settings.Subject,
                Keywords = documentModel.Settings.Keywords,
                Creator = documentModel.Settings.Creator ?? "MauiPdfGenerator", // Use provided or default
                Producer = "SkiaSharp", // SkiaSharp usually sets this, but we can be explicit
                Creation = DateTime.Now, // SkiaSharp sets this automatically
                Modified = DateTime.Now, // SkiaSharp sets this automatically
                RasterDpi = 300, // Example DPI for rasterized content (if any)
                EncodingQuality = 85, // Example JPEG quality (0-100)
                PdfA = false // Set to true for PDF/A-1b compliance (requires font embedding, etc.)
            };

            // Create the SkiaSharp PDF document targeting the output stream
            // The 'using' statement handles closing the document, which finalizes the PDF.
            using var skDocument = SKDocument.CreatePdf(outputStream, metadata) ?? throw new InvalidOperationException("SkiaSharp failed to create the PDF document.");

            // Render each page defined in the model
            foreach (var pageModel in documentModel.Pages)
            {
                // Get page size (already in points from DocumentConfigurator/PdfPageBuilder)
                PdfSize pageSizeInPoints = pageModel.Size.IsEmpty
                    ? documentModel.Settings.DefaultPageSize // Assume DefaultPageSize is also in points
                    : pageModel.Size;

                // *** Use ToSKPageSize which assumes input is points ***
                SKSize skPageSize = SkiaValueConverter.ToSKPageSize(pageSizeInPoints);

                // Begin a new page - The 'using' statement handles EndPage() for the canvas
                using var canvas = skDocument.BeginPage(skPageSize.Width, skPageSize.Height) ?? throw new InvalidOperationException($"SkiaSharp failed to begin page {documentModel.Pages.IndexOf(pageModel) + 1}.");

                // Render the content of this page using the PageRenderer
                pageRenderer.RenderPage(canvas, pageModel, fontManager, unitConverter);
            }

            // Document is automatically closed and finalized when skDocument is disposed
            // skDocument.Close(); // Not needed because of 'using' statement

        }).ConfigureAwait(false); // ConfigureAwait(false) if library doesn't need context
    }
}
