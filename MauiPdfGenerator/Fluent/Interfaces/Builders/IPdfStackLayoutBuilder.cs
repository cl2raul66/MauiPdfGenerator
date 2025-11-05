using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfStackLayoutBuilder
{
    IPdfLayoutChildParagraph Paragraph(string text);
    IPdfLayoutChildHorizontalLine HorizontalLine();
    IPdfLayoutChildImage Image(Stream stream);
    IPdfVerticalStackLayout VerticalStackLayout(Action<IPdfVerticalStackLayout> layoutSetup);
    IPdfHorizontalStackLayout HorizontalStackLayout(Action<IPdfHorizontalStackLayout> layoutSetup);
    IPdfGrid Grid(Action<IPdfGrid> layoutSetup);
}
