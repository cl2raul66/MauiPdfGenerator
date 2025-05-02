using MauiPdfGenerator.Core.Models;

namespace MauiPdfGenerator.Core;

internal interface IPdfGenerationService
{
    Task GenerateAsync(PdfDocumentData documentData, string filePath);
}
