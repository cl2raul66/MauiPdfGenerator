using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models.Views;

internal class PdfSpanData
{
    internal string Text { get; }

    internal PdfStyledProperty<PdfFontIdentifier?> FontFamilyProp { get; } = new(null);
    internal PdfStyledProperty<float?> FontSizeProp { get; } = new(null);
    internal PdfStyledProperty<Color?> TextColorProp { get; } = new(null);
    internal PdfStyledProperty<FontAttributes?> FontAttributesProp { get; } = new(null);
    internal PdfStyledProperty<TextDecorations?> TextDecorationsProp { get; } = new(null);
    internal PdfStyledProperty<TextTransform?> TextTransformProp { get; } = new(null);

    internal PdfFontRegistration? ResolvedFontRegistration { get; set; }

    internal PdfSpanData(string text)
    {
        Text = text ?? string.Empty;
    }
}
