using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Models.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IGridChildrenBuilder
{
    PdfParagraph Paragraph(string text);
    PdfImage PdfImage(Stream stream);
    PdfHorizontalLine HorizontalLine();
    PdfVerticalStackLayout VerticalStackLayout(Action<IStackLayoutBuilder> content);
    PdfHorizontalStackLayout HorizontalStackLayout(Action<IStackLayoutBuilder> content);
    PdfGrid Grid(Action<IPageContentBuilder> content);
}
