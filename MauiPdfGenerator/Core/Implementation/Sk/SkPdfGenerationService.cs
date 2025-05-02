using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class SkPdfGenerationService : IPdfGenerationService
{
    public Task GenerateAsync(PdfDocumentData documentData, string filePath)
    {
        try
        {
            var metadata = new SKDocumentPdfMetadata { /* ... metadatos ... */ };

            using var stream = new SKFileWStream(filePath);
            using var pdfDoc = SKDocument.CreatePdf(stream, metadata);

            if (pdfDoc == null) { throw new InvalidOperationException(/*...*/); }

            foreach (var pageData in documentData.Pages)
            {
                // *** Convierte ENUMS PÚBLICOS a valores SkiaSharp aquí ***
                SKSize pageSize = GetSkPageSize(pageData.Size, pageData.Orientation); // Llama al método de ayuda

                using SKCanvas pdfCanvas = pdfDoc.BeginPage(pageSize.Width, pageSize.Height);

                if (pageData.BackgroundColor != null)
                {
                    pdfCanvas.Clear(ConvertToSkColor(pageData.BackgroundColor));
                }

                // ... contenido ...

                pdfDoc.EndPage();
            }

            pdfDoc.Close();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(new PdfGenerationException($"Error generando PDF con SkiaSharp: {ex.Message}", ex));
        }
    }

    // *** MÉTODO DE AYUDA AHORA USA ENUMS PÚBLICOS DE FLUENT ***
    private SKSize GetSkPageSize(PageSizeType size, PageOrientationType orientation)
    {
        float width, height;
        switch (size) // Usa el enum PÚBLICO directamente
        {
            case PageSizeType.A4: width = 595f; height = 842f; break;
            case PageSizeType.A5: width = 420f; height = 595f; break;
            case PageSizeType.Letter: width = 612f; height = 792f; break;
            // ... otros casos ...
            case PageSizeType.Custom:
                throw new NotSupportedException("PageSizeType.Custom requiere dimensiones específicas no implementadas aún.");
            default: width = 595f; height = 842f; break; // Default A4
        }

        // Usa el enum PÚBLICO directamente para la lógica de orientación
        return orientation == PageOrientationType.Landscape ? new SKSize(height, width) : new SKSize(width, height);
    }

    private SKColor ConvertToSkColor(Color mauiColor)
    {
        // ... sin cambios ...
        return new SKColor(/* ... */);
    }
}

// ... PdfGenerationException sin cambios ...
