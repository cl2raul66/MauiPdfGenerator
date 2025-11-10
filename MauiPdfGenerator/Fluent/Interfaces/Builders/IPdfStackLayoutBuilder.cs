using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfStackLayoutBuilder
{
    IPdfLayoutChildParagraph Paragraph(string text);
    IPdfLayoutChildHorizontalLine HorizontalLine();
    IPdfLayoutChildImage Image(Stream stream);
    void VerticalStackLayout(Action<IPdfVerticalStackLayout> layoutSetup);
    void HorizontalStackLayout(Action<IPdfHorizontalStackLayout> layoutSetup);
    void Grid(Action<IPdfGrid> layoutSetup);
}
