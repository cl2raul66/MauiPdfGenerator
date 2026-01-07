using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPageContentBuilder
{
    IPdfPageChildParagraph Paragraph(string text);
    IPdfPageChildParagraph Paragraph(Action<IPdfSpanText> configure);
    IPdfPageChildHorizontalLine HorizontalLine();
    IPdfPageChildImage Image(Stream stream);
    void VerticalStackLayout(Action<IPdfVerticalStackLayout> layoutSetup);
    void HorizontalStackLayout(Action<IPdfHorizontalStackLayout> layoutSetup);
    void Grid(Action<IPdfGrid> layoutSetup);
}
