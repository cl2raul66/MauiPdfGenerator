using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPageContentBuilder
{
    IPdfParagraph Paragraph(string text);
    IPdfHorizontalLine HorizontalLine();
    IPdfImage Image(Stream stream);
    IPdfVerticalStackLayout VerticalStackLayout(Action<IStackLayoutBuilder> content);
    IPdfHorizontalStackLayout HorizontalStackLayout(Action<IStackLayoutBuilder> content);
}
