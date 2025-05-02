using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Enums;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class SkPdfGenerationService : IPdfGenerationService
{
    public Task GenerateAsync(PdfDocumentData documentData, string filePath)
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
                RasterDpi = 72,
                EncodingQuality = 100,
                PdfA = false
            };

            using var stream = new SKFileWStream(filePath);
            using var pdfDoc = SKDocument.CreatePdf(stream, metadata) ?? throw new InvalidOperationException("SkiaSharp no pudo crear el documento PDF.");

            foreach (var pageData in documentData.Pages)
            {
                SKSize pageSize = GetSkPageSize(pageData.Size, pageData.Orientation);

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
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            return Task.FromException(new PdfGenerationException($"Error generando PDF con SkiaSharp: {ex.Message}", ex));
        }
    }

    // *** MÉTODO DE AYUDA AHORA USA ENUMS PÚBLICOS DE FLUENT ***
    private SKSize GetSkPageSize(PageSizeType size, PageOrientationType orientation)
    {
        float width, height;
        switch (size) 
        {
            case PageSizeType.A4: width = 595f; height = 842f; break;
            case PageSizeType.A5: width = 420f; height = 595f; break;
            case PageSizeType.A3: width = 842f; height = 1191f; break;
            case PageSizeType.Letter: width = 612f; height = 792f; break;
            case PageSizeType.Legal: width = 612f; height = 1008f; break;
            case PageSizeType.Executive: width = 522f; height = 756f; break;
            case PageSizeType.B5: width = 499f; height = 709f; break;
            case PageSizeType.Tabloid: width = 792f; height = 1224f; break;
            // Tamaños de sobre son más complejos, pueden requerir ajustes
            case PageSizeType.Envelope_10: width = 297f; height = 684f; break; // 4 1/8 x 9 1/2 in
            case PageSizeType.Envelope_DL: width = 312f; height = 624f; break; // 110 x 220 mm
            default: width = 595f; height = 842f; break; // Default A4
        }
       
        return orientation == PageOrientationType.Landscape ? new SKSize(height, width) : new SKSize(width, height);
    }

    private SKColor ConvertToSkColor(Color mauiColor)
    {
        return new SKColor(
            (byte)(mauiColor.Red * 255),
            (byte)(mauiColor.Green * 255),
            (byte)(mauiColor.Blue * 255),
            (byte)(mauiColor.Alpha * 255)
        );
    }
}

// ... PdfGenerationException sin cambios ...
