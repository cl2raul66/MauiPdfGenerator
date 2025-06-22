using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Models.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPageContentBuilder
{
    PdfParagraph Paragraph(string text);
    PdfHorizontalLine HorizontalLine();
    PdfImage PdfImage(Stream stream);
    PdfVerticalStackLayout VerticalStackLayout(Action<IStackLayoutBuilder> content);
    PdfHorizontalStackLayout HorizontalStackLayout(Action<IStackLayoutBuilder> content);
    PdfGrid PdfGrid();
}
