using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

public interface IPdfParagraph<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf FontFamily(PdfFontIdentifier? family);
    TSelf FontSize(float size);
    TSelf TextColor(Color color);
    TSelf HorizontalTextAlignment(TextAlignment alignment);
    TSelf VerticalTextAlignment(TextAlignment alignment);
    TSelf FontAttributes(FontAttributes attributes);
    TSelf LineBreakMode(LineBreakMode mode);
    TSelf TextDecorations(TextDecorations decorations);
    TSelf TextTransform(TextTransform transform);
}

public interface IPdfParagraph : IPdfParagraph<IPdfParagraph>
{
}
