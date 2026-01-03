using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfTextStyles
{
    void ApplyFontFamily(PdfFontIdentifier? family);
    void ApplyFontSize(float size);
    void ApplyTextColor(Color color);
    void ApplyFontAttributes(FontAttributes attributes);
    void ApplyTextDecorations(TextDecorations decorations);
    void ApplyTextTransform(TextTransform transform);
    void ApplyStyle(PdfStyleIdentifier key);
}
