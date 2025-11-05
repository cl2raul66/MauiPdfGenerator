using MauiPdfGenerator.Common.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

internal interface IBuildablePdfElement
{
    PdfElementData GetModel();
}
