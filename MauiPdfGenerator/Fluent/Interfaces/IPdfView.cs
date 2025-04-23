using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfView
{
    IParagraph Paragraph(Action<IParagraph> paragraph);

    IPdfVisualElement Margins(float uniformMargin);

    IPdfVisualElement Margins(float horizontalMargin, float verticalMargin);

    IPdfVisualElement Margins(float leftMargin, float topMargin, float rightMargin, float bottomMargin);
}
