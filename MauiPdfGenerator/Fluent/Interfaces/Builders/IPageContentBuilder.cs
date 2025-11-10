using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPageContentBuilder
{
    IPdfPageChildParagraph Paragraph(string text);
    IPdfPageChildHorizontalLine HorizontalLine();
    IPdfPageChildImage Image(Stream stream);
    void VerticalStackLayout(Action<IPdfVerticalStackLayout> layoutSetup);
    void HorizontalStackLayout(Action<IPdfHorizontalStackLayout> layoutSetup);
    void Grid(Action<IPdfGrid> layoutSetup);
}
