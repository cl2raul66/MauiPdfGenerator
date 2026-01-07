using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models.Views;

internal class PdfSpanData : IPdfTextStyles
{
    internal int TextLength { get; set; }

    internal PdfStyledProperty<PdfFontIdentifier?> FontFamilyProp { get; } = new(null);
    internal PdfStyledProperty<float?> FontSizeProp { get; } = new(null);
    internal PdfStyledProperty<Color?> TextColorProp { get; } = new(null);
    internal PdfStyledProperty<FontAttributes?> FontAttributesProp { get; } = new(null);
    internal PdfStyledProperty<TextDecorations?> TextDecorationsProp { get; } = new(null);
    internal PdfStyledProperty<TextTransform?> TextTransformProp { get; } = new(null);

    internal PdfFontRegistration? ResolvedFontRegistration { get; set; }

    internal int StartIndex { get; set; }
    internal int EndIndex { get; set; }

    internal PdfFontIdentifier? CurrentFontFamily => FontFamilyProp.Value;
    internal float? CurrentFontSize => FontSizeProp.Value;
    internal Color? CurrentTextColor => TextColorProp.Value;
    internal FontAttributes? CurrentFontAttributes => FontAttributesProp.Value;
    internal TextDecorations? CurrentTextDecorations => TextDecorationsProp.Value;
    internal TextTransform? CurrentTextTransform => TextTransformProp.Value;

    internal PdfSpanData() { }

    #region IPdfTextStyles Implementation

    void IPdfTextStyles.ApplyFontFamily(PdfFontIdentifier? family)
    {
        if (family.HasValue)
        {
            FontFamilyProp.Set(family, PdfPropertyPriority.Local);
        }
    }

    void IPdfTextStyles.ApplyFontSize(float size)
    {
        FontSizeProp.Set(size, PdfPropertyPriority.Local);
    }

    void IPdfTextStyles.ApplyTextColor(Color color)
    {
        TextColorProp.Set(color, PdfPropertyPriority.Local);
    }

    void IPdfTextStyles.ApplyFontAttributes(FontAttributes attributes)
    {
        FontAttributesProp.Set(attributes, PdfPropertyPriority.Local);
    }

    void IPdfTextStyles.ApplyTextDecorations(TextDecorations decorations)
    {
        TextDecorationsProp.Set(decorations, PdfPropertyPriority.Local);
    }

    void IPdfTextStyles.ApplyTextTransform(TextTransform transform)
    {
        TextTransformProp.Set(transform, PdfPropertyPriority.Local);
    }

    void IPdfTextStyles.ApplyStyle(PdfStyleIdentifier key)
    {
    }

    #endregion
}
