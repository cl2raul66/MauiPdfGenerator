using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

public interface IPdfParagraph : IPdfElement<IPdfParagraph>
{
    IPdfParagraph FontFamily(PdfFontIdentifier? family);
    IPdfParagraph FontSize(float size);
    IPdfParagraph TextColor(Color color);
    IPdfParagraph HorizontalTextAlignment(TextAlignment alignment);
    IPdfParagraph VerticalTextAlignment(TextAlignment alignment);
    IPdfParagraph FontAttributes(FontAttributes attributes);
    IPdfParagraph LineBreakMode(LineBreakMode mode);
    IPdfParagraph TextDecorations(TextDecorations decorations);
    IPdfParagraph TextTransform(TextTransform transform);
}
