using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfStackLayoutBuilder
{
    IPdfLayoutChildParagraph Paragraph(string text);
    IPdfLayoutChildParagraph Paragraph(Action<IPdfSpanConfigurator> configure);
    IPdfLayoutChildHorizontalLine HorizontalLine();
    IPdfLayoutChildImage Image(Stream stream);
    void VerticalStackLayout(Action<IPdfVerticalStackLayout> layoutSetup);
    void HorizontalStackLayout(Action<IPdfHorizontalStackLayout> layoutSetup);
    void Grid(Action<IPdfGrid> layoutSetup);
}
