using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPageContentBuilder
{
    IPdfPageChildParagraph Paragraph(string text);
    IPdfPageChildHorizontalLine HorizontalLine();
    IPdfPageChildImage Image(Stream stream);
    IPdfVerticalStackLayout VerticalStackLayout(Action<IPdfVerticalStackLayout> layoutSetup);
    IPdfHorizontalStackLayout HorizontalStackLayout(Action<IPdfHorizontalStackLayout> layoutSetup);
    IPdfGrid PdfGrid(Action<IPdfGrid> layoutSetup);
}
