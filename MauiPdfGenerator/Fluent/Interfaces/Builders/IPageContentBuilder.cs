using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPageContentBuilder
{
    PdfParagraph Paragraph(string text);

    PdfHorizontalLine HorizontalLine();

    PdfImage PdfImage(Stream stream);
}
