using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models.Views;

internal class PdfParagraphData : PdfElementData, IPdfTextStyles
{
    public const float DefaultFontSize = 12f;
    public static readonly Color DefaultTextColor = Colors.Black;
    public const TextAlignment DefaultHorizontalTextAlignment = TextAlignment.Start;
    public const TextAlignment DefaultVerticalTextAlignment = TextAlignment.Start;
    public const FontAttributes DefaultFontAttributes = Microsoft.Maui.Controls.FontAttributes.None;
    public const LineBreakMode DefaultLineBreakMode = Microsoft.Maui.LineBreakMode.WordWrap;
    public const TextDecorations DefaultTextDecorations = Microsoft.Maui.TextDecorations.None;
    public const TextTransform DefaultTextTransform = Microsoft.Maui.TextTransform.None;
    public const float DefaultCharacterSpacing = 0f;
    public const float DefaultWordSpacing = 0f;
    public const float DefaultLineSpacing = 1.0f;

    internal string Text { get; private set; }
    internal bool IsContinuation { get; private set; } = false;

    internal string? Culture { get; set; }



    internal PdfStyledProperty<PdfFontIdentifier?> FontFamilyProp { get; } = new(null);
    internal PdfStyledProperty<float> FontSizeProp { get; } = new(DefaultFontSize);
    internal PdfStyledProperty<Color?> TextColorProp { get; } = new(null);
    internal PdfStyledProperty<TextAlignment> HorizontalTextAlignmentProp { get; } = new(DefaultHorizontalTextAlignment);
    internal PdfStyledProperty<TextAlignment> VerticalTextAlignmentProp { get; } = new(DefaultVerticalTextAlignment);
    internal PdfStyledProperty<FontAttributes> FontAttributesProp { get; } = new(DefaultFontAttributes);
    internal PdfStyledProperty<LineBreakMode> LineBreakModeProp { get; } = new(DefaultLineBreakMode);
    internal PdfStyledProperty<TextDecorations> TextDecorationsProp { get; } = new(DefaultTextDecorations);
    internal PdfStyledProperty<TextTransform> TextTransformProp { get; } = new(DefaultTextTransform);
    internal PdfStyledProperty<float> CharacterSpacingProp { get; } = new(DefaultCharacterSpacing);
    internal PdfStyledProperty<float> WordSpacingProp { get; } = new(DefaultWordSpacing);
    internal PdfStyledProperty<float> LineSpacingProp { get; } = new(DefaultLineSpacing);



    internal PdfFontIdentifier? CurrentFontFamily => FontFamilyProp.Value;
    internal float CurrentFontSize => FontSizeProp.Value;
    internal Color? CurrentTextColor => TextColorProp.Value;
    internal TextAlignment CurrentHorizontalTextAlignment => HorizontalTextAlignmentProp.Value;
    internal TextAlignment CurrentVerticalTextAlignment => VerticalTextAlignmentProp.Value;
    internal FontAttributes? CurrentFontAttributes => FontAttributesProp.Value;
    internal LineBreakMode? CurrentLineBreakMode => LineBreakModeProp.Value;
    internal TextDecorations? CurrentTextDecorations => TextDecorationsProp.Value;
    internal TextTransform? CurrentTextTransform => TextTransformProp.Value;
    internal float CurrentCharacterSpacing => CharacterSpacingProp.Value;
    internal float CurrentWordSpacing => WordSpacingProp.Value;
    internal float CurrentLineSpacing => LineSpacingProp.Value;

    internal PdfFontRegistration? ResolvedFontRegistration { get; set; }



    internal IReadOnlyList<PdfSpanData> Spans { get; private set; } = [];
    internal bool HasSpans => Spans.Count > 0;

    internal PdfParagraphData() : base() { Text = string.Empty; }
    internal PdfParagraphData(string text) : base() { Text = text ?? string.Empty; }

    internal PdfParagraphData(string text, PdfParagraphData original) : base()
    {
        Text = text ?? string.Empty;
        IsContinuation = true;
        Culture = original.Culture;

        FontFamilyProp.Set(original.FontFamilyProp.Value, PdfPropertyPriority.Local);
        FontSizeProp.Set(original.FontSizeProp.Value, PdfPropertyPriority.Local);
        TextColorProp.Set(original.TextColorProp.Value, PdfPropertyPriority.Local);
        HorizontalTextAlignmentProp.Set(original.HorizontalTextAlignmentProp.Value, PdfPropertyPriority.Local);
        VerticalTextAlignmentProp.Set(original.VerticalTextAlignmentProp.Value, PdfPropertyPriority.Local);
        FontAttributesProp.Set(original.FontAttributesProp.Value, PdfPropertyPriority.Local);
        LineBreakModeProp.Set(original.LineBreakModeProp.Value, PdfPropertyPriority.Local);
        TextDecorationsProp.Set(original.TextDecorationsProp.Value, PdfPropertyPriority.Local);
        TextTransformProp.Set(original.TextTransformProp.Value, PdfPropertyPriority.Local);
        CharacterSpacingProp.Set(original.CharacterSpacingProp.Value, PdfPropertyPriority.Local);
        WordSpacingProp.Set(original.WordSpacingProp.Value, PdfPropertyPriority.Local);
        LineSpacingProp.Set(original.LineSpacingProp.Value, PdfPropertyPriority.Local);

        ResolvedFontRegistration = original.ResolvedFontRegistration;

        MarginProp.Set(original.MarginProp.Value, PdfPropertyPriority.Local);
        PaddingProp.Set(original.PaddingProp.Value, PdfPropertyPriority.Local);
        BackgroundColorProp.Set(original.BackgroundColorProp.Value, PdfPropertyPriority.Local);
        HorizontalOptionsProp.Set(original.HorizontalOptionsProp.Value, PdfPropertyPriority.Local);
        VerticalOptionsProp.Set(original.VerticalOptionsProp.Value, PdfPropertyPriority.Local);
    }

    internal void SetSpans(IReadOnlyList<PdfSpanData> spans)
    {
        Spans = spans ?? [];
    }

    internal void SetText(string text)
    {
        Text = text ?? string.Empty;
    }

    #region IPdfTextStyles Implementation
    void IPdfTextStyles.ApplyFontFamily(PdfFontIdentifier? family)
    {
        FontFamilyProp.Set(family, PdfPropertyPriority.Local);
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

    void IPdfTextStyles.ApplyStyle(PdfStyleIdentifier key) { }
    #endregion
}
