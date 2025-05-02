using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfLayout : IPdfVisualElement
{
    IParagraph Paragraph(string text);

    IParagraph Paragraph(Action<IParagraph> paragraph);

    IPdfVerticalStackLayout PdfVerticalStackLayout(Action<IPdfVerticalStackLayout> verticalStackLayout);

    IPdfHorizontalStackLayout PdfHorizontalStackLayout(Action<IPdfHorizontalStackLayout> horizontalStackLayout);

    IPdfGrid PdfGrid(Action<IPdfGrid> grid);
}
