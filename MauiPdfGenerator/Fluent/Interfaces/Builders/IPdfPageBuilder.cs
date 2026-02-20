using MauiPdfGenerator.Common.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

internal interface IPdfPageBuilder
{
    PdfPageData BuildPageData();
}
