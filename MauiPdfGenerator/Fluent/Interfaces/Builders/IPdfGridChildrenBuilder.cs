using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfGridChildrenBuilder
{
    IPdfGridChildParagraph Paragraph(string text);
    IPdfGridChildImage Image(Stream stream);
    IPdfGridChildHorizontalLine HorizontalLine();
    IPdfGridChildVerticalStackLayout VerticalStackLayout(Action<IPdfStackLayoutBuilder> content);
    IPdfGridChildHorizontalStackLayout HorizontalStackLayout(Action<IPdfStackLayoutBuilder> content);
    IPdfGrid Grid();
}
