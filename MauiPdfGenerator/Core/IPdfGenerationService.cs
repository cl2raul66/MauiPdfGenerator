using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders; 

namespace MauiPdfGenerator.Core;

internal interface IPdfGenerationService
{
    Task GenerateAsync(PdfDocumentData documentData, string filePath, PdfFontRegistryBuilder fontRegistry);
}
