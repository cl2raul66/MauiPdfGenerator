using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Views;

public interface IPdfSpan
{
    IPdfSpan FontFamily(PdfFontIdentifier? family);
    IPdfSpan FontSize(float size);
    IPdfSpan TextColor(Color color);
    IPdfSpan FontAttributes(FontAttributes attributes);
    IPdfSpan TextDecorations(TextDecorations decorations);
    IPdfSpan TextTransform(TextTransform transform);
}
