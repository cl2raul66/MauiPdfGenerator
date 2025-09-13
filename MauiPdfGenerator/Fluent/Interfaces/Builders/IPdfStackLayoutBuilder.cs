using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfStackLayoutBuilder
{
    IPdfParagraph Paragraph(string text);
    IPdfHorizontalLine HorizontalLine();
    IPdfImage Image(Stream stream);
    IPdfVerticalStackLayout VerticalStackLayout(Action<IPdfStackLayoutBuilder> content);
    IPdfHorizontalStackLayout HorizontalStackLayout(Action<IPdfStackLayoutBuilder> content);
}
