using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPageContentBuilder
{
    IPdfPageChildParagraph Paragraph(string text);
    IPdfPageChildHorizontalLine HorizontalLine();
    IPdfPageChildImage Image(Stream stream);
    IPdfVerticalStackLayout VerticalStackLayout(Action<IPdfStackLayoutBuilder> content);
    IPdfHorizontalStackLayout HorizontalStackLayout(Action<IPdfStackLayoutBuilder> content);
    IPdfGrid Grid();
}
