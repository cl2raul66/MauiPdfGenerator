using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfGridChildrenBuilder
{
    IPdfGridChildParagraph Paragraph(string text);
    IPdfGridChildImage Image(Stream stream);
    IPdfGridChildHorizontalLine HorizontalLine();
    void VerticalStackLayout(Action<IPdfGridChildVerticalStackLayout> layoutSetup);
    void HorizontalStackLayout(Action<IPdfGridChildHorizontalStackLayout> layoutSetup);
    void Grid(Action<IPdfGrid> layoutSetup);
}
