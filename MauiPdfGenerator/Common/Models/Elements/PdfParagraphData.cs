using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models.Elements;

internal class PdfParagraphData : PdfElementData
{
    public const float DefaultFontSize = 12f;
    public static readonly Color DefaultTextColor = Colors.Black;
    public const TextAlignment DefaultHorizontalTextAlignment = TextAlignment.Start;
    public const TextAlignment DefaultVerticalTextAlignment = TextAlignment.Start;
    public const FontAttributes DefaultFontAttributes = Microsoft.Maui.Controls.FontAttributes.None;
    public const LineBreakMode DefaultLineBreakMode = Microsoft.Maui.LineBreakMode.WordWrap;
    public const TextDecorations DefaultTextDecorations = Microsoft.Maui.TextDecorations.None;
    public const TextTransform DefaultTextTransform = Microsoft.Maui.TextTransform.None;

    internal string Text { get; }
    internal PdfFontIdentifier? CurrentFontFamily { get; set; }
    internal float CurrentFontSize { get; set; }
    internal Color? CurrentTextColor { get; set; }
    internal TextAlignment CurrentHorizontalTextAlignment { get; set; }
    internal TextAlignment CurrentVerticalTextAlignment { get; set; }
    internal FontAttributes? CurrentFontAttributes { get; set; }
    internal LineBreakMode? CurrentLineBreakMode { get; set; }
    internal TextDecorations? CurrentTextDecorations { get; set; }
    internal TextTransform? CurrentTextTransform { get; set; }
    internal bool IsContinuation { get; private set; } = false;

    internal FontRegistration? ResolvedFontRegistration { get; set; }

    internal PdfParagraphData(string text)
    {
        Text = text ?? string.Empty;
        CurrentFontFamily = null;
        CurrentFontSize = 0;
        CurrentTextColor = null;
        CurrentHorizontalTextAlignment = DefaultHorizontalTextAlignment;
        CurrentVerticalTextAlignment = DefaultVerticalTextAlignment;
        CurrentFontAttributes = null;
        CurrentLineBreakMode = null;
        CurrentTextDecorations = null;
        CurrentTextTransform = null;
    }

    internal PdfParagraphData(string text, PdfParagraphData originalStyleSource)
    {
        Text = text ?? string.Empty;

        this.CurrentFontFamily = originalStyleSource.CurrentFontFamily;
        this.ResolvedFontRegistration = originalStyleSource.ResolvedFontRegistration;
        this.CurrentFontSize = originalStyleSource.CurrentFontSize;
        this.CurrentTextColor = originalStyleSource.CurrentTextColor;
        this.CurrentHorizontalTextAlignment = originalStyleSource.CurrentHorizontalTextAlignment;
        this.CurrentVerticalTextAlignment = originalStyleSource.CurrentVerticalTextAlignment;
        this.CurrentFontAttributes = originalStyleSource.CurrentFontAttributes;
        this.CurrentLineBreakMode = originalStyleSource.CurrentLineBreakMode;
        this.CurrentTextDecorations = originalStyleSource.CurrentTextDecorations;
        this.CurrentTextTransform = originalStyleSource.CurrentTextTransform;
        this.Margin(originalStyleSource.GetMargin.Left, originalStyleSource.GetMargin.Top, originalStyleSource.GetMargin.Right, originalStyleSource.GetMargin.Bottom);
        this.IsContinuation = true;
    }
}
