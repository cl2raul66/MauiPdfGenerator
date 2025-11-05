using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfGridChildrenBuilder
{
    IPdfGridChildParagraph Paragraph(string text);
    IPdfGridChildImage Image(Stream stream);
    IPdfGridChildHorizontalLine HorizontalLine();
    IPdfGridChildVerticalStackLayout VerticalStackLayout(Action<IPdfGridChildVerticalStackLayout> layoutSetup);
    IPdfGridChildHorizontalStackLayout HorizontalStackLayout(Action<IPdfGridChildHorizontalStackLayout> layoutSetup);
    IPdfGrid Grid(Action<IPdfGrid> layoutSetup);
}
