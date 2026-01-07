using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Views;

public interface IPdfParagraph<TSelf> : IPdfElement<TSelf>, IPdfStylable where TSelf : IPdfElement<TSelf>
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
    TSelf CharacterSpacing(float spacing);
    TSelf WordSpacing(float spacing);
    TSelf LineSpacing(float spacing);
}

public interface IPdfParagraph : IPdfParagraph<IPdfParagraph>
{
}
