using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPageContentBuilder
{
    PdfParagraph Paragraph(string text);

    PdfHorizontalLine HorizontalLine();

    PdfImage PdfImage(Stream stream);
    PdfImage PdfImage(Uri uri);
    PdfImage PdfImage(string source, PdfImageSourceType pdfImageSourceType);
}
