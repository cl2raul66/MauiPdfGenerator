using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPageContentBuilder
{
    PdfParagraph Paragraph(string text);

    PdfHorizontalLine HorizontalLine();

    // Future elements can be added here, e.g.:
    // PdfImage Image(string source);
    // PdfSpacer Spacer(float height);
}
