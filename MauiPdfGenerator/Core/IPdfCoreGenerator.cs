using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Fluent.Builders;

namespace MauiPdfGenerator.Core;

internal interface IPdfCoreGenerator
{
    Task GenerateAsync(PdfDocumentData documentData, string filePath, PdfFontRegistryBuilder fontRegistry);
}
