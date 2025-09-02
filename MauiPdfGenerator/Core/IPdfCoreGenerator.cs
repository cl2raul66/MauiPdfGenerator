using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Fluent.Builders;
using Microsoft.Extensions.Logging;

namespace MauiPdfGenerator.Core;

internal interface IPdfCoreGenerator
{
    Task GenerateAsync(PdfDocumentData documentData, string filePath, PdfFontRegistryBuilder fontRegistry, ILogger logger);
}
